using System;
using System.Collections.Generic;

namespace SimonDuel.Rules
{
    public enum MatchPhase { TurnReady, Repeat, AddOne, ArmChoice, RallyOver, MatchOver }

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
        public IReadOnlyList<PadId> Playback;   // what is shown: the pattern, with Echo's step doubled
        public IReadOnlyList<PadId> Entry;      // what must be pressed, in order
        public TwistId Twist;
        public int EchoStep = -1;               // the first of Echo's two steps in Playback
        public int LightMs;
        public int GapMs;
        public int TimeAllowedMs;
    }

    public enum MoveKind { Tap, TimeOut, AddStep, ArmTwist, GiveTwist }

    public readonly struct Move
    {
        public readonly MoveKind Kind;
        public readonly PadId Pad;
        public readonly int Ms;
        public readonly bool Armed;
        public readonly TwistId Twist;

        public Move(MoveKind kind, PadId pad, int ms, bool armed = false, TwistId twist = TwistId.None)
        {
            Kind = kind;
            Pad = pad;
            Ms = ms;
            Armed = armed;
            Twist = twist;
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
        readonly List<PadId> entry = new List<PadId>();
        readonly int[] score = new int[PlayerCount];
        readonly int[] charge = new int[PlayerCount];
        readonly TwistId[] held = new TwistId[PlayerCount];
        readonly Queue<TwistId> bag = new Queue<TwistId>();
        TwistId lastDrawn;
        int starter;
        int nextStep;
        int turnsDoneThisRally;

        public MatchPhase Phase { get; private set; }
        public int ActivePlayer { get; private set; }
        public int FirstStarter { get; }
        public int LastRallyWinner { get; private set; } = -1;
        public MatchRecord Record { get; }
        public IReadOnlyList<PadId> Pattern => pattern;
        public PadId Expected => entry[nextStep];
        public TwistId IncomingTwist { get; private set; }  // armed for the next turn
        public TwistId ActiveTwist { get; private set; }    // affecting the turn being played
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
        public int Charge(int player) => charge[player];
        public TwistId HeldTwist(int player) => held[player];

        public TurnPlan StartTurn()
        {
            Require(MatchPhase.TurnReady);
            nextStep = 0;
            Phase = MatchPhase.Repeat;
            ActiveTwist = IncomingTwist;
            IncomingTwist = TwistId.None;

            var playback = new List<PadId>(pattern);
            int echoStep = -1;
            if (ActiveTwist == TwistId.Echo)
            {
                echoStep = random.Next(pattern.Count);
                playback.Insert(echoStep, pattern[echoStep]);
            }

            entry.Clear();
            entry.AddRange(playback);
            if (ActiveTwist == TwistId.Reverse) entry.Reverse();

            int extraSteps = pattern.Count - rules.startingSteps;
            return new TurnPlan
            {
                Player = ActivePlayer,
                Pattern = pattern.ToArray(),
                Playback = playback,
                Entry = entry.ToArray(),
                Twist = ActiveTwist,
                EchoStep = echoStep,
                LightMs = Math.Max(rules.playbackLightFloorMs, rules.playbackLightMs - extraSteps * rules.playbackLightDropPerStepMs),
                GapMs = Math.Max(rules.playbackGapFloorMs, rules.playbackGapMs - extraSteps * rules.playbackGapDropPerStepMs),
                TimeAllowedMs = Math.Min(rules.timerMaxMs, rules.timerBaseMs + entry.Count * rules.timerPerStepMs),
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
            if (nextStep < entry.Count) return new TapVerdict(TapResult.Correct, expected);

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
            turnsDoneThisRally++;
            bool twistsAllowed = turnsDoneThisRally > rules.turnsBeforeTwists;

            if (twistsAllowed && held[ActivePlayer] == TwistId.None)
            {
                charge[ActivePlayer]++;
                if (charge[ActivePlayer] >= rules.chargeToEarnTwist) held[ActivePlayer] = DrawTwist();
            }

            if (twistsAllowed && held[ActivePlayer] != TwistId.None)
            {
                if (!rules.twistsArmThemselves)
                {
                    Phase = MatchPhase.ArmChoice;
                    return;
                }
                Arm();
            }
            HandOver();
        }

        // Uses the held Twist on the other player's next turn, or keeps it for later.
        public void ArmTwist(bool arm)
        {
            Require(MatchPhase.ArmChoice);
            Record.Moves.Add(new Move(MoveKind.ArmTwist, default, 0, arm));
            if (arm) Arm();
            HandOver();
        }

        // For testing: hands the active player a Twist straight away.
        public void GiveTwist(TwistId twist)
        {
            Record.Moves.Add(new Move(MoveKind.GiveTwist, default, 0, false, twist));
            held[ActivePlayer] = twist;
            charge[ActivePlayer] = rules.chargeToEarnTwist;
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
                    case MoveKind.ArmTwist: referee.ArmTwist(move.Armed); break;
                    case MoveKind.GiveTwist: referee.GiveTwist(move.Twist); break;
                }
            }
            return referee;
        }

        void BeginRally()
        {
            pattern.Clear();
            for (int i = 0; i < rules.startingSteps; i++)
                pattern.Add(Pads.All[random.Next(Pads.All.Length)]);
            entry.Clear();
            entry.AddRange(pattern);
            ActivePlayer = starter;
            nextStep = 0;
            turnsDoneThisRally = 0;
            IncomingTwist = TwistId.None;
            ActiveTwist = TwistId.None;
            if (rules.chargeResetsEachRally) Array.Clear(charge, 0, charge.Length);
            Phase = MatchPhase.TurnReady;
        }

        void Arm()
        {
            IncomingTwist = held[ActivePlayer];
            held[ActivePlayer] = TwistId.None;
            charge[ActivePlayer] = 0;
        }

        void HandOver()
        {
            ActivePlayer = Other(ActivePlayer);
            Phase = MatchPhase.TurnReady;
        }

        // A shuffled bag, refilled when empty, never giving the same Twist twice in a row across a refill.
        TwistId DrawTwist()
        {
            if (bag.Count == 0)
            {
                var fresh = new List<TwistId>(rules.twistBag);
                for (int i = fresh.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (fresh[i], fresh[j]) = (fresh[j], fresh[i]);
                }
                if (fresh.Count > 1 && fresh[0] == lastDrawn) (fresh[0], fresh[1]) = (fresh[1], fresh[0]);
                foreach (var twist in fresh) bag.Enqueue(twist);
            }

            lastDrawn = bag.Dequeue();
            return lastDrawn;
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
