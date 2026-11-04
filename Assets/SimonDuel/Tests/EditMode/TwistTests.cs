using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimonDuel.Rules;

namespace SimonDuel.Tests
{
    public class TwistTests
    {
        static readonly RulesConfig Rules = new RulesConfig();

        static void FinishTurn(Referee referee, bool? arm = null)
        {
            var plan = referee.StartTurn();
            foreach (var step in plan.Entry) referee.Tap(step, 100);
            referee.AddStep(PadId.Square);
            if (referee.Phase == MatchPhase.ArmChoice) referee.ArmTwist(arm ?? false);
        }

        // Finished turns until the given player holds a Twist.
        static Referee MatchWithTwistHeld(out int holder, RulesConfig rules = null)
        {
            var referee = new Referee(rules ?? Rules, 3);
            holder = referee.ActivePlayer;
            for (int turn = 0; turn < 7; turn++)
            {
                if (turn == 6)
                {
                    var plan = referee.StartTurn();
                    foreach (var step in plan.Entry) referee.Tap(step, 100);
                    referee.AddStep(PadId.Square);
                    return referee;
                }
                FinishTurn(referee);
            }
            return referee;
        }

        static Referee MatchWithIncoming(TwistId twist, out int target)
        {
            var referee = new Referee(Rules, 3);
            FinishTurn(referee);
            FinishTurn(referee);
            target = 1 - referee.ActivePlayer;
            referee.GiveTwist(twist);
            FinishTurn(referee, arm: true);
            return referee;
        }

        [Test]
        public void TheFirstTwoTurnsOfARallyGiveNoCharge()
        {
            var referee = new Referee(Rules, 3);
            int first = referee.ActivePlayer;
            FinishTurn(referee);
            FinishTurn(referee);
            Assert.AreEqual(0, referee.Charge(first));
            Assert.AreEqual(0, referee.Charge(1 - first));
        }

        [Test]
        public void EachSuccessfulTurnAfterThatGivesOnePip()
        {
            var referee = new Referee(Rules, 3);
            int first = referee.ActivePlayer;
            FinishTurn(referee);
            FinishTurn(referee);
            FinishTurn(referee);
            Assert.AreEqual(1, referee.Charge(first));
        }

        [Test]
        public void ThreePipsEarnATwist()
        {
            var referee = MatchWithTwistHeld(out int holder);
            Assert.AreNotEqual(TwistId.None, referee.HeldTwist(holder));
            Assert.AreEqual(MatchPhase.ArmChoice, referee.Phase);
        }

        [Test]
        public void KeepingATwistHoldsItForLater()
        {
            var referee = MatchWithTwistHeld(out int holder);
            var twist = referee.HeldTwist(holder);

            referee.ArmTwist(false);

            Assert.AreEqual(twist, referee.HeldTwist(holder));
            Assert.AreEqual(TwistId.None, referee.IncomingTwist);
        }

        [Test]
        public void UsingATwistSpendsItAndResetsCharge()
        {
            var referee = MatchWithTwistHeld(out int holder);
            var twist = referee.HeldTwist(holder);

            referee.ArmTwist(true);

            Assert.AreEqual(TwistId.None, referee.HeldTwist(holder));
            Assert.AreEqual(0, referee.Charge(holder));
            Assert.AreEqual(twist, referee.IncomingTwist);
        }

        [Test]
        public void AHeldTwistStopsMoreChargeSoOnlyOneIsEverHeld()
        {
            var referee = MatchWithTwistHeld(out int holder);
            var twist = referee.HeldTwist(holder);
            referee.ArmTwist(false);

            FinishTurn(referee);
            FinishTurn(referee);

            Assert.AreEqual(twist, referee.HeldTwist(holder));
            Assert.AreEqual(Rules.chargeToEarnTwist, referee.Charge(holder));
        }

        [Test]
        public void ATwistCanArmItselfWhenTheSettingSaysSo()
        {
            var rules = new RulesConfig { twistsArmThemselves = true };
            var referee = MatchWithTwistHeld(out int holder, rules);

            Assert.AreEqual(MatchPhase.TurnReady, referee.Phase);
            Assert.AreNotEqual(TwistId.None, referee.IncomingTwist);
            Assert.AreEqual(TwistId.None, referee.HeldTwist(holder));
        }

        [Test]
        public void ARallyResetsCharge()
        {
            var referee = new Referee(Rules, 3);
            int first = referee.ActivePlayer;
            FinishTurn(referee);
            FinishTurn(referee);
            FinishTurn(referee);

            referee.StartTurn();
            referee.TimeOut();
            referee.NextRally();

            Assert.AreEqual(0, referee.Charge(first));
        }

        [Test]
        public void ATwistCannotBeArmedInARallysFirstTwoTurns()
        {
            var referee = new Referee(Rules, 3);
            referee.GiveTwist(TwistId.Reverse);

            FinishTurn(referee);

            Assert.AreEqual(MatchPhase.TurnReady, referee.Phase);
            Assert.AreEqual(TwistId.None, referee.IncomingTwist);
        }

        [Test]
        public void ReverseAsksForThePatternBackToFront()
        {
            var referee = MatchWithIncoming(TwistId.Reverse, out _);

            var plan = referee.StartTurn();

            Assert.AreEqual(TwistId.Reverse, plan.Twist);
            CollectionAssert.AreEqual(plan.Pattern, plan.Playback);
            CollectionAssert.AreEqual(plan.Pattern.Reverse(), plan.Entry);
        }

        [Test]
        public void TheForwardOrderIsAWrongAnswerUnderReverse()
        {
            var referee = MatchWithIncoming(TwistId.Reverse, out _);
            var plan = referee.StartTurn();
            Assume.That(plan.Pattern[0], Is.Not.EqualTo(plan.Entry[0]));

            Assert.AreEqual(TapResult.Wrong, referee.Tap(plan.Pattern[0], 100).Result);
        }

        [Test]
        public void TheStepAddedAfterReverseStillGoesOnTheEnd()
        {
            var referee = MatchWithIncoming(TwistId.Reverse, out _);
            var plan = referee.StartTurn();
            foreach (var step in plan.Entry) referee.Tap(step, 100);

            referee.AddStep(PadId.Circle);

            Assert.AreEqual(PadId.Circle, referee.Pattern.Last());
            Assert.AreEqual(plan.Pattern.Count + 1, referee.Pattern.Count);
        }

        [Test]
        public void ShiftChangesNothingAboutWhatMustBePressed()
        {
            var referee = MatchWithIncoming(TwistId.Shift, out _);
            var plan = referee.StartTurn();
            Assert.AreEqual(TwistId.Shift, plan.Twist);
            CollectionAssert.AreEqual(plan.Pattern, plan.Entry);
        }

        [Test]
        public void EchoDoublesOneStepInPlaybackAndEntry()
        {
            var referee = MatchWithIncoming(TwistId.Echo, out _);
            var plan = referee.StartTurn();

            Assert.AreEqual(plan.Pattern.Count + 1, plan.Entry.Count);
            CollectionAssert.AreEqual(plan.Playback, plan.Entry);
            Assert.AreEqual(plan.Entry[plan.EchoStep], plan.Entry[plan.EchoStep + 1]);
            var withoutDouble = new List<PadId>(plan.Entry);
            withoutDouble.RemoveAt(plan.EchoStep);
            CollectionAssert.AreEqual(plan.Pattern, withoutDouble);
        }

        [Test]
        public void EchoLeavesThePatternItselfUnchanged()
        {
            var referee = MatchWithIncoming(TwistId.Echo, out _);
            int length = referee.Pattern.Count;
            var plan = referee.StartTurn();
            foreach (var step in plan.Entry) referee.Tap(step, 100);
            referee.AddStep(PadId.Star);
            Assert.AreEqual(length + 1, referee.Pattern.Count);
        }

        [Test]
        public void EchosExtraStepGetsExtraTime()
        {
            var referee = MatchWithIncoming(TwistId.Echo, out _);
            var plan = referee.StartTurn();
            Assert.AreEqual(3000 + 750 * plan.Entry.Count, plan.TimeAllowedMs);
        }

        [Test]
        public void ATwistLastsOneTurn()
        {
            var referee = MatchWithIncoming(TwistId.Reverse, out _);
            FinishTurn(referee);
            var next = referee.StartTurn();
            Assert.AreEqual(TwistId.None, next.Twist);
            CollectionAssert.AreEqual(next.Pattern, next.Entry);
        }

        [Test]
        public void TheBagDealsEveryTwistBeforeRepeating()
        {
            var rules = new RulesConfig { chargeToEarnTwist = 1, turnsBeforeTwists = 0, chargeResetsEachRally = false };
            var referee = new Referee(rules, 21);
            var dealt = new List<TwistId>();
            for (int turn = 0; turn < 9; turn++)
            {
                int player = referee.ActivePlayer;
                var plan = referee.StartTurn();
                foreach (var step in plan.Entry) referee.Tap(step, 100);
                referee.AddStep(PadId.Circle);
                dealt.Add(referee.HeldTwist(player));
                referee.ArmTwist(true);
            }

            CollectionAssert.AreEquivalent(new[] { TwistId.Reverse, TwistId.Shift, TwistId.Echo }, dealt.Take(3));
            CollectionAssert.AreEquivalent(new[] { TwistId.Reverse, TwistId.Shift, TwistId.Echo }, dealt.Skip(3).Take(3));
            for (int i = 1; i < dealt.Count; i++) Assert.AreNotEqual(dealt[i - 1], dealt[i]);
        }

        [Test]
        public void AMatchWithTwistsReplaysExactly()
        {
            var referee = MatchWithIncoming(TwistId.Echo, out _);
            FinishTurn(referee);
            FinishTurn(referee);
            referee.StartTurn();
            referee.TimeOut();

            var replayed = Referee.Replay(Rules, referee.Record);

            CollectionAssert.AreEqual(referee.Pattern, replayed.Pattern);
            Assert.AreEqual(referee.Score(0), replayed.Score(0));
            Assert.AreEqual(referee.Score(1), replayed.Score(1));
        }

        [Test]
        public void TheSummaryNamesTheTwistOnEachTurn()
        {
            var referee = MatchWithIncoming(TwistId.Reverse, out _);
            FinishTurn(referee);
            var summary = MatchSummary.Build(Rules, referee.Record);
            Assert.AreEqual("Reverse", summary.turns.Last().twist);
        }
    }
}
