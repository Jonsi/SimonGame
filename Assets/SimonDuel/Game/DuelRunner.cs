using System;
using System.Collections;
using SimonDuel.Rules;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace SimonDuel.Game
{
    // Plays one match on screen: asks the referee what happens, and shows it in order.
    // Turn order: whose turn, click when ready, watch, short pause, repeat, add one, hand over.
    public class DuelRunner : MonoBehaviour
    {
        [SerializeField] DuelRules rules;
        [SerializeField] Timing timing;
        [SerializeField] PadBoard board;
        [SerializeField] DuelScreen screen;
        [SerializeField] Boot boot;

        const int NoActivePlayer = -1;

        enum Stage { BeforeStart, Waiting, ReadyToWatch, Watching, Repeating, AddingOne, ChoosingTwist, MatchResult }

        Referee referee;
        Stage stage = Stage.BeforeStart;
        bool paused;
        float timeAllowed, timeLeft, nextLogAt, matchStartedAt;
        int turn;

        void Awake()
        {
            screen.Listen(Rematch, () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex), MatchNotes.Download);
            screen.ListenToTwists(() => DecideTwist(true), () => DecideTwist(false));
        }

        // Every press comes here first. Returns true when it was used up and must not reach a pad.
        public bool PressAnywhere()
        {
            if (boot.TryStart())
            {
                Begin(new Referee(rules.rules, (ulong)DateTime.UtcNow.Ticks));
                return true;
            }
            if (paused)
            {
                SetPaused(false);
                return true;
            }
            if (stage != Stage.ReadyToWatch) return false;

            stage = Stage.Watching;
            return true;
        }

        public void PressPad(Pad pad)
        {
            if (paused) return;

            if (stage == Stage.Repeating) Tap(pad);
            else if (stage == Stage.AddingOne) AddStep(pad);
        }

        void Begin(Referee match)
        {
            referee = match;
            turn = 0;
            matchStartedAt = Time.time;
            Debug.Log($"[Duel] match starts seed={referee.Record.Seed} first=P{referee.FirstStarter + 1}");
            screen.HideResult();
            StartCoroutine(PlayMatch());
        }

        void Rematch()
        {
            Begin(referee.Rematch());
        }

        IEnumerator PlayMatch()
        {
            while (true)
            {
                yield return PlayTurn();
                if (referee.Phase == MatchPhase.MatchOver) break;
                if (referee.Phase == MatchPhase.RallyOver) referee.NextRally();
            }

            stage = Stage.MatchResult;
            Debug.Log($"[Duel] match over winner=P{referee.Winner + 1} score={referee.Score(0)}-{referee.Score(1)} moves={referee.Record.Moves.Count}");
            MatchNotes.Add(MatchSummary.Build(rules.rules, referee.Record), Mathf.RoundToInt(Time.time - matchStartedAt));
            screen.ShowResult(referee.Winner, referee.Score(0), referee.Score(1), MatchNotes.Count);
        }

        IEnumerator PlayTurn()
        {
            turn++;
            string name = $"PLAYER {referee.ActivePlayer + 1}";
            stage = Stage.Waiting;
            ShowPlayers(referee.ActivePlayer);
            screen.ShowLength(referee.Pattern.Count);
            screen.ShowTimerDirection(false);
            screen.ShowTimer(1f);
            screen.ShowBanner(name);
            yield return new WaitForSeconds(timing.ownershipSeconds);

            if (referee.IncomingTwist != TwistId.None) yield return AnnounceTwist(referee.IncomingTwist);

            screen.ShowBanner($"{name}: CLICK WHEN READY");
            stage = Stage.ReadyToWatch;
            while (stage == Stage.ReadyToWatch) yield return null;

            var plan = referee.StartTurn();
            Debug.Log($"[Duel] turn={turn} P{plan.Player + 1} watch len={plan.Pattern.Count} twist={plan.Twist} light={plan.LightMs}ms gap={plan.GapMs}ms time={plan.TimeAllowedMs}ms");
            screen.ShowBanner("WATCH");
            yield return PlayPattern(plan);

            if (plan.Twist == TwistId.Shift)
            {
                screen.ShowBanner("SHIFT!");
                board.ShiftClockwise(timing.shiftSeconds);
                yield return new WaitForSeconds(timing.shiftSeconds);
            }
            yield return new WaitForSeconds(timing.readyBeatSeconds);

            screen.ShowBanner(RepeatWord(plan.Twist));
            screen.ShowTimerDirection(plan.Twist == TwistId.Reverse);
            timeAllowed = timeLeft = plan.TimeAllowedMs / 1000f;
            nextLogAt = Time.time + 1f;
            stage = Stage.Repeating;
            while (stage == Stage.Repeating) yield return null;

            if (referee.Phase == MatchPhase.AddOne)
            {
                screen.ShowTimerDirection(false);
                screen.ShowTimer(1f);
                screen.ShowBanner("ADD ONE");
                stage = Stage.AddingOne;
                while (stage == Stage.AddingOne) yield return null;
                ShowPlayers(plan.Player);
                screen.ShowLength(referee.Pattern.Count);
                yield return new WaitForSeconds(timing.addOneHoldSeconds);
                if (referee.Phase == MatchPhase.ArmChoice) yield return OfferTwist();
            }
            else
            {
                yield return new WaitForSeconds(timing.rallyLossRevealSeconds);
                ShowPlayers(NoActivePlayer);
                screen.ShowBanner($"POINT TO PLAYER {referee.LastRallyWinner + 1}");
                yield return new WaitForSeconds(timing.pointCelebrationSeconds);
            }

            if (plan.Twist == TwistId.Shift)
            {
                board.ShiftHome(timing.shiftSeconds);
                yield return new WaitForSeconds(timing.shiftSeconds);
            }
        }

        IEnumerator PlayPattern(TurnPlan plan)
        {
            for (int i = 0; i < plan.Playback.Count; i++)
            {
                bool echoed = plan.EchoStep >= 0 && (i == plan.EchoStep || i == plan.EchoStep + 1);
                board.Find(plan.Playback[i]).Play(plan.LightMs / 1000f, echoed ? board.TwistColor : Color.white);

                float gap = i == plan.EchoStep ? timing.echoGapSeconds : plan.GapMs / 1000f;
                yield return new WaitForSeconds(plan.LightMs / 1000f + gap);
            }
        }

        IEnumerator AnnounceTwist(TwistId twist)
        {
            screen.ShowBanner(TwistSentence(twist));
            Debug.Log($"[Duel] twist incoming {twist}");

            string seenKey = $"CuzGames.SeenTwist.{twist}";
            if (PlayerPrefs.GetInt(seenKey, 0) == 1)
            {
                yield return new WaitForSeconds(timing.twistNameSeconds);
                yield break;
            }

            PlayerPrefs.SetInt(seenKey, 1);
            PlayerPrefs.Save();
            yield return ShowDemo(twist);
            yield return new WaitForSeconds(timing.twistFirstTimeSeconds);
        }

        // A short showing of the rule on the pads, the first time a Twist appears.
        IEnumerator ShowDemo(TwistId twist)
        {
            float step = timing.demoStepSeconds;
            if (twist == TwistId.Reverse)
            {
                var order = new[] { PadId.Circle, PadId.Triangle, PadId.Star, PadId.Triangle, PadId.Circle };
                foreach (var pad in order)
                {
                    board.Find(pad).Play(step);
                    yield return new WaitForSeconds(step);
                }
            }
            else if (twist == TwistId.Shift)
            {
                board.ShiftClockwise(timing.shiftSeconds);
                yield return new WaitForSeconds(timing.shiftSeconds + step);
                board.ShiftHome(timing.shiftSeconds);
                yield return new WaitForSeconds(timing.shiftSeconds);
            }
            else if (twist == TwistId.Echo)
            {
                for (int i = 0; i < 2; i++)
                {
                    board.Find(PadId.Star).Play(step, board.TwistColor);
                    yield return new WaitForSeconds(step + timing.echoGapSeconds);
                }
            }
        }

        IEnumerator OfferTwist()
        {
            int player = referee.ActivePlayer;
            var twist = referee.HeldTwist(player);
            stage = Stage.ChoosingTwist;
            ShowPlayers(player, player);
            screen.ShowBanner($"USE {TwistName(twist)} ON THE OTHER PLAYER?");

            float left = timing.twistChoiceSeconds;
            while (stage == Stage.ChoosingTwist)
            {
                left -= Time.deltaTime;
                if (left <= 0f) DecideTwist(false);
                yield return null;
            }

            ShowPlayers(player);
            if (referee.IncomingTwist == TwistId.None) yield break;

            screen.ShowBanner($"{TwistName(twist)} ARMED!");
            yield return new WaitForSeconds(timing.twistNameSeconds);
        }

        void DecideTwist(bool use)
        {
            if (stage != Stage.ChoosingTwist) return;

            var twist = referee.HeldTwist(referee.ActivePlayer);
            referee.ArmTwist(use);
            Debug.Log($"[Duel] twist {twist} {(use ? "armed" : "kept")}");
            stage = Stage.Waiting;
        }

        static string TwistName(TwistId twist) => twist.ToString().ToUpperInvariant();

        static string TwistSentence(TwistId twist)
        {
            switch (twist)
            {
                case TwistId.Reverse: return "REVERSE: REPEAT IT BACK TO FRONT";
                case TwistId.Shift: return "SHIFT: THE PADS MOVE AFTER YOU WATCH";
                default: return "ECHO: ONE STEP PLAYS TWICE, PRESS IT TWICE";
            }
        }

        static string RepeatWord(TwistId twist)
        {
            switch (twist)
            {
                case TwistId.Reverse: return "REPEAT: BACK TO FRONT";
                case TwistId.Shift: return "REPEAT: THE PADS MOVED";
                case TwistId.Echo: return "REPEAT: PRESS IT TWICE";
                default: return "REPEAT";
            }
        }

        void Tap(Pad pad)
        {
            int ms = Mathf.RoundToInt((timeAllowed - timeLeft) * 1000f);
            int entered = referee.StepsEntered;
            var verdict = referee.Tap(pad.Id, ms);
            if (verdict.Result == TapResult.Wrong)
            {
                pad.ShowWrong();
                EndRally("SLIP!", verdict.Expected, $"slip step={entered + 1} tapped={pad.Id}");
                return;
            }

            pad.Press();
            if (verdict.Result == TapResult.Complete) stage = Stage.Waiting;
        }

        void AddStep(Pad pad)
        {
            referee.AddStep(pad.Id);
            pad.Press();
            Debug.Log($"[Duel] add one pad={pad.Id} len={referee.Pattern.Count}");
            stage = Stage.Waiting;
        }

        void EndRally(string word, PadId expected, string why)
        {
            board.Find(expected).Play(timing.rallyLossRevealSeconds);
            screen.ShowBanner(word);
            Debug.Log($"[Duel] rally ends {why} expected={expected} point=P{referee.LastRallyWinner + 1} score={referee.Score(0)}-{referee.Score(1)}");
            stage = Stage.Waiting;
        }

        void ShowPlayers(int activePlayer, int offeredTo = NoActivePlayer)
        {
            screen.ShowPlayers(activePlayer, referee, offeredTo);
        }

        void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PretendPlayers();
#endif
            if (stage != Stage.Repeating) return;

            timeLeft -= Time.deltaTime;
            screen.ShowTimer(Mathf.Clamp01(timeLeft / timeAllowed));
            if (timeLeft > 0f)
            {
                LogEachSecond();
                return;
            }

            var expected = referee.Expected;
            referee.TimeOut();
            EndRally("TOO SLOW!", expected, "timeout");
        }

        void LogEachSecond()
        {
            if (Time.time < nextLogAt) return;
            nextLogAt += 1f;
            Debug.Log($"[Duel] turn={turn} P{referee.ActivePlayer + 1} len={referee.Pattern.Count} phase=Repeat taps={referee.StepsEntered}/{referee.Pattern.Count} left={timeLeft:0.0}s");
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && referee != null && stage != Stage.MatchResult) SetPaused(true);
        }

        void SetPaused(bool pause)
        {
            paused = pause;
            Time.timeScale = pause ? 0f : 1f;
            AudioListener.pause = pause;
            screen.ShowPaused(pause);
            Debug.Log($"[Duel] {(pause ? "paused" : "resumed")} turn={turn}");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Testing only: F1 plays it right, F2 slips, F3 runs out of time. R, S and E hand over a Twist.
        void PretendPlayers()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || paused) return;

            if (referee != null && stage != Stage.MatchResult)
            {
                if (keyboard.rKey.wasPressedThisFrame) GiveTwist(TwistId.Reverse);
                if (keyboard.sKey.wasPressedThisFrame) GiveTwist(TwistId.Shift);
                if (keyboard.eKey.wasPressedThisFrame) GiveTwist(TwistId.Echo);
            }

            if (keyboard.f1Key.wasPressedThisFrame)
            {
                if (stage == Stage.ReadyToWatch) stage = Stage.Watching;
                else if (stage == Stage.ChoosingTwist) DecideTwist(true);
                else if (stage == Stage.AddingOne) AddStep(board.Find(referee.Pattern[0]));
                else
                    while (stage == Stage.Repeating) Tap(board.Find(referee.Expected));
            }
            else if (keyboard.f2Key.wasPressedThisFrame && stage == Stage.Repeating)
            {
                Tap(board.Find((PadId)(((int)referee.Expected + 1) % Pads.All.Length)));
            }
            else if (keyboard.f3Key.wasPressedThisFrame && stage == Stage.Repeating)
            {
                timeLeft = 0f;
            }
        }

        void GiveTwist(TwistId twist)
        {
            referee.GiveTwist(twist);
            ShowPlayers(referee.ActivePlayer);
        }
#endif
    }
}
