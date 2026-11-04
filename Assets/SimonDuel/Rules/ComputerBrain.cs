using System;
using System.Collections.Generic;

namespace SimonDuel.Rules
{
    public readonly struct ComputerTap
    {
        public readonly PadId Pad;
        public readonly int DelayMs;

        public ComputerTap(PadId pad, int delayMs)
        {
            Pad = pad;
            DelayMs = delayMs;
        }
    }

    // Plays like a person with a memory limit: sure of short patterns, shaky past its comfort,
    // and likelier to lose the middle of a pattern than its start or end.
    public sealed class ComputerBrain
    {
        readonly ComputerLevel level;
        readonly SeededRandom random;

        public ComputerBrain(ComputerLevel level, ulong seed)
        {
            this.level = level;
            random = new SeededRandom(seed);
        }

        // The taps for one turn. If it slips, the list ends on the wrong pad.
        public List<ComputerTap> PlanTurn(TurnPlan plan)
        {
            var taps = new List<ComputerTap>();
            int length = plan.Pattern.Count;
            int pastComfort = Math.Max(0, length - level.comfortableLength);

            for (int i = 0; i < plan.Entry.Count; i++)
            {
                int delay = random.Next(level.tapGapMaxMs - level.tapGapMinMs + 1) + level.tapGapMinMs;
                delay += pastComfort * level.tapGapPastComfortMs;
                if (random.Chance(level.hesitationChance)) delay += level.hesitationMs;

                if (random.Chance(SlipChance(plan, i, pastComfort)))
                {
                    taps.Add(new ComputerTap(WrongPad(plan.Entry[i]), delay));
                    return taps;
                }
                taps.Add(new ComputerTap(plan.Entry[i], delay));
            }
            return taps;
        }

        public PadId ChooseStep(IReadOnlyList<PadId> pattern)
        {
            int count = pattern.Count;
            var last = pattern[count - 1];
            switch (level.addStyle)
            {
                case AddStyle.AvoidEasyRuns:
                    return WrongPad(last);

                case AddStyle.Tricky:
                    if (count >= 2 && random.Chance(0.5)) return pattern[count - 2];  // keeps an A-B-A-B run going
                    if (random.Chance(0.5)) return last;                              // doubles a step
                    return AnyPad();

                default:
                    return AnyPad();
            }
        }

        public bool ShouldUseTwist(int patternLength)
        {
            return patternLength >= level.saveTwistUntilLength;
        }

        double SlipChance(TurnPlan plan, int step, int pastComfort)
        {
            double chance = level.slipChanceWhenComfortable + level.slipGrowthPastComfort * pastComfort * pastComfort;
            if (plan.Twist != TwistId.None) chance += level.slipChanceUnderTwist;

            int last = Math.Max(1, plan.Entry.Count - 1);
            double fromEdge = 1d - Math.Abs(2d * step / last - 1d);
            chance *= 1d + (level.middleOfPatternWeight - 1d) * fromEdge;
            return Math.Min(0.95d, chance);
        }

        PadId AnyPad() => Pads.All[random.Next(Pads.All.Length)];

        PadId WrongPad(PadId pad)
        {
            int offset = random.Next(Pads.All.Length - 1) + 1;
            return Pads.All[((int)pad + offset) % Pads.All.Length];
        }
    }
}
