using System;
using System.Collections;
using SimonDuel.Rules;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace SimonDuel.Game
{
    // Plays one match or practice run on screen: asks the referee what happens, and shows it in order.
    // A human turn: whose turn, click when ready, watch, short pause, repeat, add one, hand over.
    public class DuelRunner : MonoBehaviour
    {
        [SerializeField] DuelRules rules;
        [SerializeField] ComputerLevels computerLevels;
        [SerializeField] Timing timing;
        [SerializeField] PadBoard board;
        [SerializeField] DuelScreen screen;
        [SerializeField] SoundBoard sounds;
        [SerializeField] Effects effects;
        [SerializeField] Feel feel;
        [SerializeField] Tutorial tutorial;

        const int NoActivePlayer = -1;
        const int ComputerSeat = 1;

        enum Stage { Menu, Tutorial, Waiting, Watching, Repeating, ComputerRepeating, AddingOne, ChoosingTwist, MatchResult }

        readonly string[] names = new string[Referee.PlayerCount];
        Referee referee;
        ComputerBrain brain;
        PlaySetup setup;
        Stage stage = Stage.Menu;
        bool paused;
        float timeAllowed, timeUsed, nextLogAt, matchStartedAt;
        int turn;

        bool Practising => setup.mode == GameMode.Practice;

        void Awake()
        {
            screen.Listen(Rematch, GoToMenu, MatchNotes.Download);
            screen.ListenToTwists(() => DecideTwist(true), () => DecideTwist(false));
            screen.ListenToMenu(Play);
            screen.ListenToPause(() => SetPaused(true), () => SetPaused(false), QuitToMenu, ShowTutorial);
            GameSettings.Apply();
            screen.ShowMenu(true);
            screen.ShowFirstTimeHint(!Tutorial.Done);
        }

        public bool PressIsOnPauseButton(Vector2 screenPoint)
        {
            return screen.IsOverPauseButton(screenPoint);
        }

        public void PressPad(Pad pad)
        {
            if (paused) return;

            if (stage == Stage.Repeating) Tap(pad);
            else if (stage == Stage.AddingOne) AddStep(pad);
            else if (stage == Stage.Tutorial) tutorial.Press(pad);
        }

        void ShowTutorial()
        {
            stage = Stage.Tutorial;
            screen.ShowMenu(false);
            StartCoroutine(PlayTutorial());
        }

        IEnumerator PlayTutorial()
        {
            yield return tutorial.Run();
            GoToMenu();
        }

        void GoToMenu()
        {
            sounds.Play(GameSound.Click);
            stage = Stage.Menu;
            referee = null;
            screen.HideResult();
            screen.ShowMenu(true);
            screen.ShowFirstTimeHint(!Tutorial.Done);
        }

        void Play(PlaySetup chosen)
        {
            sounds.Play(GameSound.Click);
            setup = chosen;
            screen.ShowMenu(false);
            Begin(NewReferee());
        }

        Referee NewReferee()
        {
            ulong seed = (ulong)DateTime.UtcNow.Ticks;
            if (!Practising) return new Referee(rules.rules, seed);

            var practice = rules.rules.Copy();
            practice.practiceAlone = true;
            practice.replayEachTurn = true;
            practice.noTimer = setup.practice == PracticeKind.Classic;
            practice.twistEveryTurn = setup.practice == PracticeKind.Twists;
            return new Referee(practice, seed, 0);
        }

        void Begin(Referee match)
        {
            referee = match;
            turn = 0;
            matchStartedAt = Time.time;
            brain = setup.mode == GameMode.Computer
                ? new ComputerBrain(computerLevels.levels[setup.computerLevel], referee.Record.Seed + 1)
                : null;

            names[0] = setup.mode == GameMode.TwoPlayers ? "PLAYER 1" : "YOU";
            names[1] = setup.mode == GameMode.Computer ? "COMPUTER" : "PLAYER 2";
            board.SetSilent(Practising && setup.practice == PracticeKind.SightOnly);
            screen.ShowNames(names, setup.mode == GameMode.Computer ? "C" : "2", !Practising);
            screen.HideResult();

            Debug.Log($"[Duel] starts mode={ModeName()} seed={referee.Record.Seed} first=P{referee.FirstStarter + 1}");
            StartCoroutine(Practising ? PlayPractice() : PlayMatch());
        }

        void QuitToMenu()
        {
            StopAllCoroutines();
            SetPaused(false);
            board.ShiftHome(0.01f);
            GoToMenu();
        }

        void Rematch()
        {
            sounds.Play(GameSound.Click);
            Begin(Practising ? NewReferee() : referee.Rematch());
        }

        string ModeName()
        {
            switch (setup.mode)
            {
                case GameMode.Computer: return $"Computer{computerLevels.levels[setup.computerLevel].label}";
                case GameMode.Practice: return $"Practice{setup.practice}";
                default: return "TwoPlayers";
            }
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
            Debug.Log($"[Duel] match over winner={names[referee.Winner]} score={referee.Score(0)}-{referee.Score(1)} moves={referee.Record.Moves.Count}");

            var summary = MatchSummary.Build(referee.Rules, referee.Record);
            summary.mode = ModeName();
            MatchNotes.Add(summary, Mathf.RoundToInt(Time.time - matchStartedAt));

            string title = referee.Winner == 0 && setup.mode == GameMode.Computer ? "YOU WIN" : $"{names[referee.Winner]} WINS";
            sounds.Play(GameSound.Win);
            effects.Confetti(new[] { feel.playerColors[referee.Winner], feel.looks[1].rest, feel.looks[2].rest, feel.looks[3].rest });
            screen.ShowResult(title, referee.Winner, $"{referee.Score(0)} - {referee.Score(1)}", "REMATCH", MatchNotes.Count);
        }

        IEnumerator PlayPractice()
        {
            ShowPlayers(0);
            screen.ShowBanner("GET READY");
            screen.ShowLength(referee.Pattern.Count);
            screen.ShowTimerDirection(false);
            yield return SweepRing();

            while (referee.Phase != MatchPhase.MatchOver)
            {
                turn++;
                stage = Stage.Waiting;
                screen.ShowLength(referee.Pattern.Count);
                screen.ShowTimerDirection(false);
                screen.ShowTimer(1f);
                if (referee.IncomingTwist != TwistId.None) yield return AnnounceTwist(referee.IncomingTwist);

                var plan = referee.StartTurn();
                yield return WatchAndPause(plan);
                OpenRepeat(plan, Stage.Repeating);
                while (stage == Stage.Repeating) yield return null;

                if (referee.Phase == MatchPhase.AddOne)
                {
                    screen.ShowBanner("NICE!");
                    referee.AddRandomStep();
                    yield return new WaitForSeconds(timing.addOneHoldSeconds);
                }
                else
                {
                    yield return new WaitForSeconds(timing.rallyLossRevealSeconds);
                }
                yield return SlideHomeAfter(plan);
            }

            stage = Stage.MatchResult;
            int reached = referee.Pattern.Count - 1;
            string bestKey = $"CuzGames.Best.{setup.practice}";
            int best = Mathf.Max(reached, PlayerPrefs.GetInt(bestKey, 0));
            PlayerPrefs.SetInt(bestKey, best);
            PlayerPrefs.Save();
            Debug.Log($"[Duel] practice over kind={setup.practice} reached={reached} best={best}");
            screen.ShowResult($"YOU REMEMBERED {reached}", 0, $"Best {best}", "AGAIN", 0);
        }

        IEnumerator PlayTurn()
        {
            turn++;
            int player = referee.ActivePlayer;
            bool computer = setup.mode == GameMode.Computer && player == ComputerSeat;

            stage = Stage.Waiting;
            ShowPlayers(player);
            screen.ShowLength(referee.Pattern.Count);
            screen.ShowTimerDirection(false);
            screen.ShowBanner(setup.mode == GameMode.Computer && player == 0 ? "YOUR TURN" : $"{names[player]}'S TURN");
            sounds.Play(player == 0 ? GameSound.TurnOne : GameSound.TurnTwo);
            yield return SweepRing();

            if (referee.IncomingTwist != TwistId.None) yield return AnnounceTwist(referee.IncomingTwist);

            var plan = referee.StartTurn();
            Debug.Log($"[Duel] turn={turn} {names[player]} watch len={plan.Pattern.Count} twist={plan.Twist} light={plan.LightMs}ms gap={plan.GapMs}ms time={plan.TimeAllowedMs}ms");
            yield return WatchAndPause(plan);

            if (computer)
            {
                OpenRepeat(plan, Stage.ComputerRepeating);
                foreach (var tap in brain.PlanTurn(plan))
                {
                    yield return new WaitForSeconds(tap.DelayMs / 1000f);
                    if (stage != Stage.ComputerRepeating) break;
                    Tap(board.Find(tap.Pad));
                }
            }
            else
            {
                OpenRepeat(plan, Stage.Repeating);
                while (stage == Stage.Repeating) yield return null;
            }

            if (referee.Phase == MatchPhase.AddOne)
            {
                screen.ShowTimerDirection(false);
                screen.ShowTimer(1f);
                screen.ShowBanner(computer ? "COMPUTER: ADD ONE" : "ADD ONE");

                if (computer)
                {
                    stage = Stage.Waiting;
                    yield return new WaitForSeconds(timing.addOneHoldSeconds);
                    AddStep(board.Find(brain.ChooseStep(referee.Pattern)));
                }
                else
                {
                    stage = Stage.AddingOne;
                    while (stage == Stage.AddingOne) yield return null;
                }

                ShowPlayers(plan.Player);
                screen.ShowLength(referee.Pattern.Count);
                yield return ShowNewStep();
                if (referee.Phase == MatchPhase.ArmChoice) yield return OfferTwist(computer);
            }
            else
            {
                yield return new WaitForSeconds(timing.rallyLossRevealSeconds);
                ShowPlayers(NoActivePlayer);
                screen.ShowBanner($"POINT TO {names[referee.LastRallyWinner]}");
                sounds.Play(GameSound.Point);
                yield return new WaitForSeconds(timing.pointCelebrationSeconds);
            }

            yield return SlideHomeAfter(plan);
        }

        // The added step is the only new thing, so it always shows at one fixed pace.
        // Pressing fast can never hide it.
        IEnumerator ShowNewStep()
        {
            stage = Stage.Waiting;
            screen.ShowBanner("NEW STEP");
            yield return new WaitForSeconds(timing.addOneHoldSeconds);
            board.Find(referee.Pattern[referee.Pattern.Count - 1]).Play(timing.newStepSeconds);
            yield return new WaitForSeconds(timing.newStepSeconds + timing.addOneHoldSeconds);
        }

        // The ring fills in the new player's color as the turn changes hands.
        IEnumerator SweepRing()
        {
            float seconds = GameSettings.ReduceMotion ? 0f : feel.handOverSeconds;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                screen.ShowTimer(t / seconds);
                yield return null;
            }
            screen.ShowTimer(1f);
            yield return new WaitForSeconds(Mathf.Max(0f, timing.ownershipSeconds - seconds));
        }

        IEnumerator WatchAndPause(TurnPlan plan)
        {
            stage = Stage.Watching;
            if (!plan.ShowPattern)
            {
                yield return ShowEchoStep(plan);
                yield return ShiftPads(plan);
                yield break;
            }

            screen.ShowBanner("WATCH");
            bool lightsOff = Practising && setup.practice == PracticeKind.SoundOnly;
            float pace = GameSettings.SlowPlayback ? timing.slowPlaybackFactor : 1f;
            float light = plan.LightMs / 1000f * pace;

            for (int i = 0; i < plan.Playback.Count; i++)
            {
                var pad = board.Find(plan.Playback[i]);
                bool echoed = plan.EchoStep >= 0 && (i == plan.EchoStep || i == plan.EchoStep + 1);
                if (lightsOff) pad.PlayToneOnly();
                else pad.Play(light, echoed ? board.TwistColor : Color.white);

                float gap = i == plan.EchoStep ? timing.echoGapSeconds : plan.GapMs / 1000f * pace;
                yield return new WaitForSeconds(light + gap);
            }

            yield return ShiftPads(plan);
            yield return new WaitForSeconds(timing.readyBeatSeconds);
        }

        // With no replay, Echo points out its doubled step on its own.
        IEnumerator ShowEchoStep(TurnPlan plan)
        {
            if (plan.Twist != TwistId.Echo) yield break;

            screen.ShowBanner($"STEP {plan.EchoStep + 1} COUNTS TWICE");
            var pad = board.Find(plan.Playback[plan.EchoStep]);
            for (int i = 0; i < 2; i++)
            {
                pad.Play(timing.demoStepSeconds, board.TwistColor);
                yield return new WaitForSeconds(timing.demoStepSeconds + timing.echoGapSeconds);
            }
        }

        IEnumerator ShiftPads(TurnPlan plan)
        {
            if (plan.Twist != TwistId.Shift) yield break;

            screen.ShowBanner("SHIFT!");
            sounds.Play(GameSound.Twist);
            board.ShiftClockwise(ShiftTime());
            yield return new WaitForSeconds(timing.shiftSeconds);
        }

        void OpenRepeat(TurnPlan plan, Stage repeating)
        {
            screen.ShowBanner(RepeatWord(plan.Twist));
            screen.ShowTimerDirection(plan.Twist == TwistId.Reverse);
            timeAllowed = plan.TimeAllowedMs / 1000f;
            timeUsed = 0f;
            nextLogAt = Time.time + 1f;
            stage = repeating;
        }

        IEnumerator SlideHomeAfter(TurnPlan plan)
        {
            if (plan.Twist != TwistId.Shift) yield break;
            board.ShiftHome(ShiftTime());
            yield return new WaitForSeconds(timing.shiftSeconds);
        }

        float ShiftTime() => GameSettings.ReduceMotion ? 0.01f : timing.shiftSeconds;

        IEnumerator AnnounceTwist(TwistId twist)
        {
            screen.ShowBanner(TwistSentence(twist));
            sounds.Play(GameSound.Twist);
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
                board.ShiftClockwise(ShiftTime());
                yield return new WaitForSeconds(timing.shiftSeconds + step);
                board.ShiftHome(ShiftTime());
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

        IEnumerator OfferTwist(bool computer)
        {
            int player = referee.ActivePlayer;
            var twist = referee.HeldTwist(player);
            stage = Stage.ChoosingTwist;

            if (computer)
            {
                DecideTwist(brain.ShouldUseTwist(referee.Pattern.Count));
            }
            else
            {
                ShowPlayers(player, player);
                screen.ShowBanner($"USE {TwistName(twist)} ON {names[1 - player]}?");

                float left = timing.twistChoiceSeconds;
                while (stage == Stage.ChoosingTwist)
                {
                    left -= Time.deltaTime;
                    if (left <= 0f) DecideTwist(false);
                    yield return null;
                }
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
                case TwistId.Shift: return "SHIFT: THE PADS MOVE, THE PATTERN DOESN'T";
                default: return "ECHO: ONE STEP COUNTS TWICE, PRESS IT TWICE";
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
            int entered = referee.StepsEntered;
            var verdict = referee.Tap(pad.Id, Mathf.RoundToInt(timeUsed * 1000f));
            if (verdict.Result == TapResult.Wrong)
            {
                pad.ShowWrong();
                effects.Burst(pad.transform.position, feel.wrongRimColor, feel.wrongBurstPieces);
                EndRally("SLIP!", verdict.Expected, $"slip step={entered + 1} tapped={pad.Id}");
                return;
            }

            pad.Press();
            effects.Burst(pad.transform.position, feel.looks[(int)pad.Id].lit, feel.burstPieces);
            if (verdict.Result != TapResult.Complete) return;

            sounds.Play(GameSound.Done);
            stage = Stage.Waiting;
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
            sounds.Play(GameSound.Wrong);
            screen.ShowBanner(word);
            screen.ShowRingError();
            Debug.Log($"[Duel] rally ends {why} expected={expected} score={referee.Score(0)}-{referee.Score(1)}");
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
            if (stage != Stage.Repeating && stage != Stage.ComputerRepeating) return;

            timeUsed += Time.deltaTime;
            if (timeAllowed <= 0f) return;

            float left = timeAllowed - timeUsed;
            screen.ShowTimer(Mathf.Clamp01(left / timeAllowed));
            if (left > 0f)
            {
                LogEachSecond(left);
                return;
            }

            var expected = referee.Expected;
            referee.TimeOut();
            EndRally("TOO SLOW!", expected, "timeout");
        }

        void LogEachSecond(float left)
        {
            if (Time.time < nextLogAt) return;
            nextLogAt += 1f;
            Debug.Log($"[Duel] turn={turn} {names[referee.ActivePlayer]} len={referee.Pattern.Count} phase=Repeat taps={referee.StepsEntered} left={left:0.0}s");
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && referee != null && stage != Stage.MatchResult && stage != Stage.Menu) SetPaused(true);
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
                if (stage == Stage.ChoosingTwist) DecideTwist(true);
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
                timeUsed = timeAllowed;
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
