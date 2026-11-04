using System.Linq;
using NUnit.Framework;
using SimonDuel.Rules;

namespace SimonDuel.Tests
{
    public class MatchSummaryTests
    {
        static readonly RulesConfig Rules = new RulesConfig();

        static PadId WrongPad(PadId pad) => Pads.All.First(p => p != pad);

        // Rally 1: a finished turn, then a slip on the second step. Rally 2: a timeout.
        static Referee PlayMatch()
        {
            var referee = new Referee(Rules, 5);

            var plan = referee.StartTurn();
            referee.Tap(plan.Pattern[0], 400);
            referee.Tap(plan.Pattern[1], 900);
            referee.AddStep(PadId.Star);

            plan = referee.StartTurn();
            referee.Tap(plan.Pattern[0], 300);
            referee.Tap(WrongPad(plan.Pattern[1]), 700);

            referee.NextRally();
            referee.StartTurn();
            referee.TimeOut();
            return referee;
        }

        [Test]
        public void EveryTurnIsListedWithHowItEnded()
        {
            var summary = MatchSummary.Build(Rules, PlayMatch().Record);

            CollectionAssert.AreEqual(
                new[] { TurnSummary.Finished, TurnSummary.Slip, TurnSummary.Timeout },
                summary.turns.Select(t => t.result));
            CollectionAssert.AreEqual(new[] { 1, 1, 2 }, summary.turns.Select(t => t.rally));
            CollectionAssert.AreEqual(new[] { 2, 3, 2 }, summary.turns.Select(t => t.length));
        }

        [Test]
        public void TimeUsedAndSlipStepAreKept()
        {
            var turns = MatchSummary.Build(Rules, PlayMatch().Record).turns;

            Assert.AreEqual(900, turns[0].usedMs);
            Assert.AreEqual(4500, turns[0].allowedMs);
            Assert.AreEqual(2, turns[1].slipStep);
            Assert.AreEqual(4500, turns[2].usedMs);
        }

        [Test]
        public void WinnerAndScoreMatchTheMatch()
        {
            var referee = PlayMatch();
            var summary = MatchSummary.Build(Rules, referee.Record);

            Assert.AreEqual(referee.Winner, summary.winner);
            Assert.AreEqual(referee.Score(0), summary.score0);
            Assert.AreEqual(referee.Score(1), summary.score1);
        }
    }
}
