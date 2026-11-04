using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimonDuel.Rules;

namespace SimonDuel.Tests
{
    public class ComputerTests
    {
        static readonly RulesConfig Rules = new RulesConfig();
        const int Matches = 1000;

        struct Result
        {
            public int Winner;
            public int LongestPattern;
        }

        static Result PlayMatch(ComputerLevel first, ComputerLevel second, ulong seed)
        {
            var referee = new Referee(Rules, seed);
            var brains = new[] { new ComputerBrain(first, seed + 1), new ComputerBrain(second, seed + 2) };
            int longest = 0;

            while (referee.Phase != MatchPhase.MatchOver)
            {
                if (referee.Phase == MatchPhase.RallyOver) referee.NextRally();

                var brain = brains[referee.ActivePlayer];
                var plan = referee.StartTurn();
                longest = Math.Max(longest, plan.Pattern.Count);
                foreach (var tap in brain.PlanTurn(plan)) referee.Tap(tap.Pad, 100);

                if (referee.Phase != MatchPhase.AddOne) continue;
                referee.AddStep(brain.ChooseStep(referee.Pattern));
                if (referee.Phase == MatchPhase.ArmChoice) referee.ArmTwist(brain.ShouldUseTwist(referee.Pattern.Count));
            }

            return new Result { Winner = referee.Winner, LongestPattern = longest };
        }

        static double WinShare(ComputerLevel a, ComputerLevel b)
        {
            int wins = 0;
            for (int i = 0; i < Matches; i++)
            {
                bool aFirstSeat = i % 2 == 0;
                var result = aFirstSeat ? PlayMatch(a, b, (ulong)i * 7 + 1) : PlayMatch(b, a, (ulong)i * 7 + 1);
                if (result.Winner == (aFirstSeat ? 0 : 1)) wins++;
            }
            return wins / (double)Matches;
        }

        static double AverageLongest(ComputerLevel level)
        {
            return Enumerable.Range(0, Matches).Average(i => PlayMatch(level, level, (ulong)i * 3 + 1).LongestPattern);
        }

        [Test]
        public void HardClearlyBeatsEasy()
        {
            Assert.Greater(WinShare(ComputerLevel.Hard(), ComputerLevel.Easy()), 0.85);
        }

        [Test]
        public void HardBeatsMedium()
        {
            Assert.Greater(WinShare(ComputerLevel.Hard(), ComputerLevel.Medium()), 0.7);
        }

        [Test]
        public void MediumBeatsEasy()
        {
            Assert.Greater(WinShare(ComputerLevel.Medium(), ComputerLevel.Easy()), 0.7);
        }

        [Test]
        public void HigherLevelsReachLongerPatterns()
        {
            double easy = AverageLongest(ComputerLevel.Easy());
            double medium = AverageLongest(ComputerLevel.Medium());
            double hard = AverageLongest(ComputerLevel.Hard());
            Assert.Greater(medium, easy + 1.5, $"easy {easy} medium {medium}");
            Assert.Greater(hard, medium + 1.5, $"medium {medium} hard {hard}");
        }

        [Test]
        public void ItAlmostNeverSlipsWellInsideItsComfort()
        {
            var brain = new ComputerBrain(ComputerLevel.Hard(), 4);
            var plan = new Referee(Rules, 1).StartTurn();
            int slips = Enumerable.Range(0, 1000).Count(_ => !brain.PlanTurn(plan).Select(t => t.Pad).SequenceEqual(plan.Entry));
            Assert.Less(slips, 30);
        }

        [Test]
        public void SlipsClusterInTheMiddleOfThePattern()
        {
            var rules = new RulesConfig { startingSteps = 9 };
            var shaky = ComputerLevel.Easy();
            shaky.comfortableLength = 20;
            shaky.slipChanceWhenComfortable = 0.02;
            var brain = new ComputerBrain(shaky, 4);
            var plan = new Referee(rules, 1).StartTurn();
            var slipAt = new int[plan.Entry.Count];

            for (int i = 0; i < 4000; i++)
            {
                var taps = brain.PlanTurn(plan);
                if (taps.Last().Pad != plan.Entry[taps.Count - 1]) slipAt[taps.Count - 1]++;
            }

            Assert.Greater(slipAt[3] + slipAt[4] + slipAt[5], slipAt[0] + slipAt[1] + slipAt[2]);
        }

        [Test]
        public void GapsBetweenTapsLookHuman()
        {
            var brain = new ComputerBrain(ComputerLevel.Medium(), 4);
            var plan = new Referee(Rules, 1).StartTurn();
            var gaps = Enumerable.Range(0, 200).SelectMany(_ => brain.PlanTurn(plan)).Select(t => t.DelayMs).ToList();
            Assert.GreaterOrEqual(gaps.Min(), 280);
            Assert.Greater(gaps.Distinct().Count(), 20);
        }

        [Test]
        public void EasyUsesTwistsAtOnceWhileHardSavesThem()
        {
            Assert.IsTrue(new ComputerBrain(ComputerLevel.Easy(), 1).ShouldUseTwist(3));
            Assert.IsFalse(new ComputerBrain(ComputerLevel.Hard(), 1).ShouldUseTwist(8));
            Assert.IsTrue(new ComputerBrain(ComputerLevel.Hard(), 1).ShouldUseTwist(11));
        }

        [Test]
        public void HardBuildsRunsThatRepeat()
        {
            var brain = new ComputerBrain(ComputerLevel.Hard(), 9);
            var pattern = new List<PadId> { PadId.Circle, PadId.Star };
            for (int i = 0; i < 500; i++) pattern.Add(brain.ChooseStep(pattern));

            int repeats = Enumerable.Range(2, pattern.Count - 2).Count(i => pattern[i] == pattern[i - 2] || pattern[i] == pattern[i - 1]);
            Assert.Greater(repeats, 300);
        }

        [Test]
        public void PracticeAloneKeepsTheSamePlayerAndEndsOnASlip()
        {
            var rules = new RulesConfig { practiceAlone = true };
            var referee = new Referee(rules, 2, 0);

            var plan = referee.StartTurn();
            foreach (var step in plan.Entry) referee.Tap(step, 100);
            referee.AddRandomStep();

            Assert.AreEqual(0, referee.ActivePlayer);
            Assert.AreEqual(3, referee.Pattern.Count);

            plan = referee.StartTurn();
            referee.Tap(Pads.All.First(p => p != plan.Entry[0]), 100);
            Assert.AreEqual(MatchPhase.MatchOver, referee.Phase);
            Assert.AreEqual(-1, referee.Winner);
        }

        [Test]
        public void ClassicPracticeHasNoTimer()
        {
            var referee = new Referee(new RulesConfig { practiceAlone = true, noTimer = true }, 2, 0);
            Assert.AreEqual(0, referee.StartTurn().TimeAllowedMs);
        }

        [Test]
        public void TwistPracticeGivesATwistEveryTurn()
        {
            var referee = new Referee(new RulesConfig { practiceAlone = true, twistEveryTurn = true }, 2, 0);
            var plan = referee.StartTurn();
            foreach (var step in plan.Entry) referee.Tap(step, 100);
            referee.AddRandomStep();
            Assert.AreNotEqual(TwistId.None, referee.IncomingTwist);
        }

        [Test]
        public void APracticeRunReplaysExactly()
        {
            var rules = new RulesConfig { practiceAlone = true, twistEveryTurn = true };
            var referee = new Referee(rules, 2, 0);
            for (int i = 0; i < 4; i++)
            {
                var plan = referee.StartTurn();
                foreach (var step in plan.Entry) referee.Tap(step, 100);
                referee.AddRandomStep();
            }

            CollectionAssert.AreEqual(referee.Pattern, Referee.Replay(rules, referee.Record).Pattern);
        }
    }
}
