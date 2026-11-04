using System;
using System.Collections.Generic;

namespace SimonDuel.Rules
{
    public enum MatchPhase { TurnReady, Repeat, AddOne, RallyOver, MatchOver }

    public enum TapResult { Ignored, Correct, Complete, Wrong }

    public readonly struct TapVerdict
    {
        public readonly TapResult Result;
        public readonly PadId Expected;

        public TapVerdict(TapResult result, PadId expected)
        {
            Result = result;
            Expected = expected;
        }
    }

    // What one turn asks of the player.
    public sealed class TurnPlan
    {
        public int Player;
        public IReadOnlyList<PadId> Pattern;
        public int LightMs;
        public int GapMs;
        public int TimeAllowedMs;
    }

    public enum MoveKind { Tap, TimeOut, AddStep }

    public readonly struct Move
    {
        public readonly MoveKind Kind;
        public readonly PadId Pad;
        public readonly int Ms;

        public Move(MoveKind kind, PadId pad, int ms)
        {
            Kind = kind;
            Pad = pad;
            Ms = ms;
        }
    }

    // Enough to play a match again exactly.
    public sealed class MatchRecord
    {
        public readonly ulong Seed;
        public readonly int FirstStarter;
        public readonly List<Move> Moves = new List<Move>();

        public MatchRecord(ulong seed, int firstStarter)
        {
            Seed = seed;
            FirstStarter = firstStarter;
        }
    }

    // Runs one match. Knows only pad identities and whole milliseconds, never the screen or the clock.
    public sealed class Referee
    {
        public const int PlayerCount = 2;

        readonly RulesConfig rules;
        readonly SeededRandom random;
        readonly List<PadId> pattern = new List<PadId>();
        readonly int[] score = new int[PlayerCount];
        int starter;
        int nextStep;

        public MatchPhase Phase { get; private set; }
        public int ActivePlayer { get; private set; }
        public int FirstStarter { get; }
        public int LastRallyWinner { get; private set; } = -1;
        public MatchRecord Record { get; }
        public IReadOnlyList<PadId> Pattern => pattern;
        public PadId Expected => pattern[nextStep];
        public int StepsEntered => nextStep;
        public int Winner => Phase == MatchPhase.MatchOver ? LastRallyWinner : -1;

        public Referee(RulesConfig rules, ulong seed, int? firstStarter = null)
        {
            this.rules = rules;
            random = new SeededRandom(seed);
            int drawn = random.Next(PlayerCount); // always drawn, so a chosen starter leaves the pattern unchanged
            FirstStarter = firstStarter ?? drawn;
            starter = FirstStarter;
            Record = new MatchRecord(seed, FirstStarter);
            BeginRally();
        }

        public int Score(int player) => score[player];

        public TurnPlan StartTurn()
        {
            Require(MatchPhase.TurnReady);
            nextStep = 0;
            Phase = MatchPhase.Repeat;

            int extraSteps = pattern.Count - rules.startingSteps;
            return new TurnPlan
            {
                Player = ActivePlayer,
                Pattern = pattern.ToArray(),
                LightMs = Math.Max(rules.playbackLightFloorMs, rules.playbackLightMs - extraSteps * rules.playbackLightDropPerStepMs),
                GapMs = Math.Max(rules.playbackGapFloorMs, rules.playbackGapMs - extraSteps * rules.playbackGapDropPerStepMs),
                TimeAllowedMs = Math.Min(rules.timerMaxMs, rules.timerBaseMs + pattern.Count * rules.timerPerStepMs),
            };
        }

        public TapVerdict Tap(PadId pad, int msSinceInputOpened)
        {
            if (Phase != MatchPhase.Repeat) return new TapVerdict(TapResult.Ignored, pad);

            Record.Moves.Add(new Move(MoveKind.Tap, pad, msSinceInputOpened));
            var expected = Expected;
            if (pad != expected)
            {
                EndRally();
                return new TapVerdict(TapResult.Wrong, expected);
            }

            nextStep++;
            if (nextStep < pattern.Count) return new TapVerdict(TapResult.Correct, expected);

            Phase = MatchPhase.AddOne;
            return new TapVerdict(TapResult.Complete, expected);
        }

        public void TimeOut()
        {
            Require(MatchPhase.Repeat);
            Record.Moves.Add(new Move(MoveKind.TimeOut, default, 0));
            EndRally();
        }

        public void AddStep(PadId pad)
        {
            Require(MatchPhase.AddOne);
            Record.Moves.Add(new Move(MoveKind.AddStep, pad, 0));
            pattern.Add(pad);
            ActivePlayer = Other(ActivePlayer);
            Phase = MatchPhase.TurnReady;
        }

        public void NextRally()
        {
            Require(MatchPhase.RallyOver);
            starter = Other(starter);
            BeginRally();
        }

        public Referee Rematch()
        {
            Require(MatchPhase.MatchOver);
            return new Referee(rules, random.NextValue(), Other(FirstStarter));
        }

        public static Referee Replay(RulesConfig rules, MatchRecord record)
        {
            var referee = new Referee(rules, record.Seed, record.FirstStarter);
            foreach (var move in record.Moves)
            {
                if (referee.Phase == MatchPhase.RallyOver) referee.NextRally();
                if (referee.Phase == MatchPhase.TurnReady) referee.StartTurn();

                switch (move.Kind)
                {
                    case MoveKind.Tap: referee.Tap(move.Pad, move.Ms); break;
                    case MoveKind.TimeOut: referee.TimeOut(); break;
                    case MoveKind.AddStep: referee.AddStep(move.Pad); break;
                }
            }
            return referee;
        }

        void BeginRally()
        {
            pattern.Clear();
            for (int i = 0; i < rules.startingSteps; i++)
                pattern.Add(Pads.All[random.Next(Pads.All.Length)]);
            ActivePlayer = starter;
            nextStep = 0;
            Phase = MatchPhase.TurnReady;
        }

        void EndRally()
        {
            LastRallyWinner = Other(ActivePlayer);
            score[LastRallyWinner]++;
            Phase = score[LastRallyWinner] >= rules.ralliesToWin ? MatchPhase.MatchOver : MatchPhase.RallyOver;
        }

        void Require(MatchPhase phase)
        {
            if (Phase != phase) throw new InvalidOperationException($"Expected {phase} but the match is in {Phase}.");
        }

        static int Other(int player) => (player + 1) % PlayerCount;
    }
}
