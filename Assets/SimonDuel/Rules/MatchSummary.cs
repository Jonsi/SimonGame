using System;
using System.Collections.Generic;

namespace SimonDuel.Rules
{
    // One turn as it happened, for tuning speeds and timers from real play.
    [Serializable]
    public struct TurnSummary
    {
        public const string Finished = "finished";
        public const string Slip = "slip";
        public const string Timeout = "timeout";

        public int rally;
        public int player;
        public int length;
        public int allowedMs;
        public int usedMs;
        public string result;
        public int slipStep;
        public string twist;
    }

    // A short account of one finished match.
    [Serializable]
    public sealed class MatchSummary
    {
        public string finishedAtUtc;
        public int seconds;
        public int winner;
        public int score0;
        public int score1;
        public List<TurnSummary> turns = new List<TurnSummary>();

        public static MatchSummary Build(RulesConfig rules, MatchRecord record)
        {
            var summary = new MatchSummary();
            var referee = new Referee(rules, record.Seed, record.FirstStarter);
            var turn = new TurnSummary();
            int rally = 1;

            foreach (var move in record.Moves)
            {
                if (referee.Phase == MatchPhase.RallyOver)
                {
                    referee.NextRally();
                    rally++;
                }
                if (referee.Phase == MatchPhase.TurnReady)
                {
                    var plan = referee.StartTurn();
                    turn = new TurnSummary { rally = rally, player = plan.Player, length = plan.Pattern.Count, allowedMs = plan.TimeAllowedMs, twist = plan.Twist.ToString() };
                }

                switch (move.Kind)
                {
                    case MoveKind.Tap:
                        int step = referee.StepsEntered + 1;
                        turn.usedMs = move.Ms;
                        if (referee.Tap(move.Pad, move.Ms).Result == TapResult.Wrong)
                        {
                            turn.result = TurnSummary.Slip;
                            turn.slipStep = step;
                            summary.turns.Add(turn);
                        }
                        break;

                    case MoveKind.TimeOut:
                        referee.TimeOut();
                        turn.usedMs = turn.allowedMs;
                        turn.result = TurnSummary.Timeout;
                        summary.turns.Add(turn);
                        break;

                    case MoveKind.AddStep:
                        referee.AddStep(move.Pad);
                        turn.result = TurnSummary.Finished;
                        summary.turns.Add(turn);
                        break;

                    case MoveKind.ArmTwist:
                        referee.ArmTwist(move.Armed);
                        break;

                    case MoveKind.GiveTwist:
                        referee.GiveTwist(move.Twist);
                        break;
                }
            }

            summary.winner = referee.Winner;
            summary.score0 = referee.Score(0);
            summary.score1 = referee.Score(1);
            return summary;
        }
    }
}
