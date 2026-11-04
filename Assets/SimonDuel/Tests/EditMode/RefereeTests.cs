using System;
using System.Linq;
using NUnit.Framework;
using SimonDuel.Rules;

namespace SimonDuel.Tests
{
    public class RefereeTests
    {
        static readonly RulesConfig Rules = new RulesConfig();

        static Referee NewMatch(ulong seed = 7) => new Referee(Rules, seed);

        static PadId WrongPad(PadId pad) => Pads.All.First(p => p != pad);

        // Plays the whole pattern right, then adds a step.
        static void PlayGoodTurn(Referee referee, PadId added = PadId.Star)
        {
            var plan = referee.StartTurn();
            foreach (var step in plan.Pattern) referee.Tap(step, 100);
            referee.AddStep(added);
        }

        static void Slip(Referee referee)
        {
            referee.StartTurn();
            referee.Tap(WrongPad(referee.Expected), 100);
        }

        [Test]
        public void SameSeedGivesTheSameMatchStart()
        {
            var a = NewMatch(42);
            var b = NewMatch(42);
            Assert.AreEqual(a.FirstStarter, b.FirstStarter);
            CollectionAssert.AreEqual(a.Pattern, b.Pattern);
        }

        [Test]
        public void DifferentSeedsVary()
        {
            var patterns = Enumerable.Range(0, 50).Select(i => string.Join(",", NewMatch((ulong)i).Pattern));
            Assert.Greater(patterns.Distinct().Count(), 1);
        }

        [Test]
        public void EveryFirstStarterCanHappen()
        {
            var starters = Enumerable.Range(0, 50).Select(i => NewMatch((ulong)i).FirstStarter).Distinct();
            CollectionAssert.AreEquivalent(new[] { 0, 1 }, starters);
        }

        [Test]
        public void ARallyStartsWithTwoSteps()
        {
            Assert.AreEqual(2, NewMatch().Pattern.Count);
        }

        [Test]
        public void TimerGrowsWithLengthUpToTheLimit()
        {
            var referee = NewMatch();
            Assert.AreEqual(4500, referee.StartTurn().TimeAllowedMs);

            var longRules = new RulesConfig { startingSteps = 15 };
            Assert.AreEqual(14000, new Referee(longRules, 1).StartTurn().TimeAllowedMs);
        }

        [Test]
        public void PlaybackSpeedsUpButNeverPassesTheFloor()
        {
            var first = NewMatch().StartTurn();
            Assert.AreEqual(420, first.LightMs);
            Assert.AreEqual(140, first.GapMs);

            var steepRules = new RulesConfig { playbackLightDropPerStepMs = 500, playbackGapDropPerStepMs = 500 };
            var referee = new Referee(steepRules, 1);
            PlayGoodTurn(referee);
            var longPlan = referee.StartTurn();
            Assert.AreEqual(320, longPlan.LightMs);
            Assert.AreEqual(100, longPlan.GapMs);
        }

        [Test]
        public void RightTapsAreCorrectThenComplete()
        {
            var referee = NewMatch();
            var plan = referee.StartTurn();
            Assert.AreEqual(TapResult.Correct, referee.Tap(plan.Pattern[0], 100).Result);
            Assert.AreEqual(TapResult.Complete, referee.Tap(plan.Pattern[1], 200).Result);
            Assert.AreEqual(MatchPhase.AddOne, referee.Phase);
        }

        [Test]
        public void AWrongTapEndsTheRallyAndNamesTheExpectedPad()
        {
            var referee = NewMatch();
            int active = referee.ActivePlayer;
            var expected = referee.StartTurn().Pattern[0];

            var verdict = referee.Tap(WrongPad(expected), 100);

            Assert.AreEqual(TapResult.Wrong, verdict.Result);
            Assert.AreEqual(expected, verdict.Expected);
            Assert.AreEqual(1, referee.Score(1 - active));
            Assert.AreEqual(MatchPhase.RallyOver, referee.Phase);
        }

        [Test]
        public void TapsOutsideRepeatAreIgnored()
        {
            var referee = NewMatch();
            Assert.AreEqual(TapResult.Ignored, referee.Tap(PadId.Circle, 0).Result);
            Assert.AreEqual(0, referee.Record.Moves.Count);
        }

        [Test]
        public void AddingAStepGrowsThePatternAndHandsOver()
        {
            var referee = NewMatch();
            int active = referee.ActivePlayer;

            PlayGoodTurn(referee, PadId.Square);

            Assert.AreEqual(3, referee.Pattern.Count);
            Assert.AreEqual(PadId.Square, referee.Pattern[2]);
            Assert.AreEqual(1 - active, referee.ActivePlayer);
        }

        [Test]
        public void RunningOutOfTimeLosesTheRally()
        {
            var referee = NewMatch();
            int active = referee.ActivePlayer;
            referee.StartTurn();

            referee.TimeOut();

            Assert.AreEqual(1, referee.Score(1 - active));
        }

        [Test]
        public void StarterAlternatesEachRally()
        {
            var referee = NewMatch();
            int first = referee.ActivePlayer;
            Slip(referee);
            referee.NextRally();
            Assert.AreEqual(1 - first, referee.ActivePlayer);
        }

        [Test]
        public void ANewRallyStartsAFreshPattern()
        {
            var referee = NewMatch();
            PlayGoodTurn(referee);
            Slip(referee);
            referee.NextRally();
            Assert.AreEqual(2, referee.Pattern.Count);
        }

        [Test]
        public void FirstToTwoRalliesWins()
        {
            var referee = NewMatch();
            int loser = referee.ActivePlayer;

            Slip(referee);
            referee.NextRally();
            PlayGoodTurn(referee);
            Assert.AreEqual(loser, referee.ActivePlayer);
            Slip(referee);

            Assert.AreEqual(MatchPhase.MatchOver, referee.Phase);
            Assert.AreEqual(1 - loser, referee.Winner);
            Assert.AreEqual(2, referee.Score(1 - loser));
        }

        [Test]
        public void RematchFlipsWhoStarts()
        {
            var referee = NewMatch();
            int first = referee.FirstStarter;
            int loser = referee.ActivePlayer;
            Slip(referee);
            referee.NextRally();
            PlayGoodTurn(referee);
            Slip(referee);

            var rematch = referee.Rematch();

            Assert.AreEqual(1 - first, rematch.FirstStarter);
            Assert.AreEqual(1 - first, rematch.ActivePlayer);
            Assert.AreEqual(0, rematch.Score(loser));
        }

        [Test]
        public void TurnsCannotBeStartedTwice()
        {
            var referee = NewMatch();
            referee.StartTurn();
            Assert.Throws<InvalidOperationException>(() => referee.StartTurn());
        }

        // Pretend players: follows a script of 'right', 'slip' and 'late' turns to the end of a match.
        static Referee PlayScript(ulong seed, string script)
        {
            var referee = new Referee(Rules, seed);
            foreach (char turn in script)
            {
                if (referee.Phase == MatchPhase.MatchOver) break;
                if (referee.Phase == MatchPhase.RallyOver) referee.NextRally();

                if (turn == 'r') PlayGoodTurn(referee, Pads.All[referee.Pattern.Count % Pads.All.Length]);
                else if (turn == 's') Slip(referee);
                else
                {
                    referee.StartTurn();
                    referee.TimeOut();
                }
            }
            return referee;
        }

        [Test]
        public void AWholeMatchEndsWithTheRightWinner()
        {
            var referee = PlayScript(11, "rrslrrs");
            int starter = referee.FirstStarter;

            // Rally 1: first player right, second right, first slips: a point to the second player.
            // Rally 2: the second player starts and runs out of time: a point to the first player.
            // Rally 3: the first player starts, is right, then the second player is right, then the first slips.
            Assert.AreEqual(MatchPhase.MatchOver, referee.Phase);
            Assert.AreEqual(1 - starter, referee.Winner);
            Assert.AreEqual(1, referee.Score(starter));
            Assert.AreEqual(2, referee.Score(1 - starter));
        }

        [Test]
        public void ARecordedMatchReplaysExactly()
        {
            var played = PlayScript(99, "rrrsrlrrs");

            var replayed = Referee.Replay(Rules, played.Record);

            Assert.AreEqual(played.Phase, replayed.Phase);
            Assert.AreEqual(played.Score(0), replayed.Score(0));
            Assert.AreEqual(played.Score(1), replayed.Score(1));
            Assert.AreEqual(played.Winner, replayed.Winner);
            CollectionAssert.AreEqual(played.Pattern, replayed.Pattern);
        }
    }
}
