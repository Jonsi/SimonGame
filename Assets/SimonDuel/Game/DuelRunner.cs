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

        enum Stage { BeforeStart, Waiting, ReadyToWatch, Watching, Repeating, AddingOne, MatchResult }

        Referee referee;
        Stage stage = Stage.BeforeStart;
        bool paused;
        float timeAllowed, timeLeft, nextLogAt;
        int turn;

        void Awake()
        {
            screen.Listen(Rematch, () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
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
            screen.ShowResult(referee.Winner, referee.Score(0), referee.Score(1));
        }

        IEnumerator PlayTurn()
        {
            turn++;
            string name = $"PLAYER {referee.ActivePlayer + 1}";
            stage = Stage.Waiting;
            ShowPlayers(referee.ActivePlayer);
            screen.ShowLength(referee.Pattern.Count);
            screen.ShowTimer(1f);
            screen.ShowBanner(name);
            yield return new WaitForSeconds(timing.ownershipSeconds);

            screen.ShowBanner($"{name}: CLICK WHEN READY");
            stage = Stage.ReadyToWatch;
            while (stage == Stage.ReadyToWatch) yield return null;

            var plan = referee.StartTurn();
            Debug.Log($"[Duel] turn={turn} P{plan.Player + 1} watch len={plan.Pattern.Count} light={plan.LightMs}ms gap={plan.GapMs}ms time={plan.TimeAllowedMs}ms");
            screen.ShowBanner("WATCH");
            foreach (var step in plan.Pattern)
            {
                board.Find(step).Play(plan.LightMs / 1000f);
                yield return new WaitForSeconds((plan.LightMs + plan.GapMs) / 1000f);
            }
            yield return new WaitForSeconds(timing.readyBeatSeconds);

            screen.ShowBanner("REPEAT");
            timeAllowed = timeLeft = plan.TimeAllowedMs / 1000f;
            nextLogAt = Time.time + 1f;
            stage = Stage.Repeating;
            while (stage == Stage.Repeating) yield return null;

            if (referee.Phase == MatchPhase.AddOne)
            {
                screen.ShowTimer(1f);
                screen.ShowBanner("ADD ONE");
                stage = Stage.AddingOne;
                while (stage == Stage.AddingOne) yield return null;
                yield return new WaitForSeconds(timing.addOneHoldSeconds);
                yield break;
            }

            yield return new WaitForSeconds(timing.rallyLossRevealSeconds);
            ShowPlayers(NoActivePlayer);
            screen.ShowBanner($"POINT TO PLAYER {referee.LastRallyWinner + 1}");
            yield return new WaitForSeconds(timing.pointCelebrationSeconds);
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

        void ShowPlayers(int activePlayer)
        {
            screen.ShowPlayers(activePlayer, referee.Score(0), referee.Score(1));
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
        // Testing only: F1 plays it right, F2 slips, F3 runs out of time.
        void PretendPlayers()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || paused) return;

            if (keyboard.f1Key.wasPressedThisFrame)
            {
                if (stage == Stage.ReadyToWatch) stage = Stage.Watching;
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
#endif
    }
}
