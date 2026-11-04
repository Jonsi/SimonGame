using System;

namespace SimonDuel.Rules
{
    public enum AddStyle { Random, AvoidEasyRuns, Tricky }

    // Every number behind one level of the computer player.
    [Serializable]
    public sealed class ComputerLevel
    {
        public string label;
        public int comfortableLength;
        public double slipChanceWhenComfortable;
        public double slipGrowthPastComfort;    // grows with the square of the steps past comfort
        public double slipChanceUnderTwist;
        public double middleOfPatternWeight;    // how much likelier a slip is mid-pattern than at the ends
        public int tapGapMinMs;
        public int tapGapMaxMs;
        public int tapGapPastComfortMs;
        public double hesitationChance;
        public int hesitationMs;
        public AddStyle addStyle;
        public int saveTwistUntilLength;

        public static ComputerLevel Easy() => new ComputerLevel
        {
            label = "EASY", comfortableLength = 5, slipChanceWhenComfortable = 0.012, slipGrowthPastComfort = 0.02,
            slipChanceUnderTwist = 0.07, middleOfPatternWeight = 2.2, tapGapMinMs = 340, tapGapMaxMs = 650,
            tapGapPastComfortMs = 120, hesitationChance = 0.14, hesitationMs = 420, addStyle = AddStyle.Random, saveTwistUntilLength = 0,
        };

        public static ComputerLevel Medium() => new ComputerLevel
        {
            label = "MEDIUM", comfortableLength = 8, slipChanceWhenComfortable = 0.006, slipGrowthPastComfort = 0.02,
            slipChanceUnderTwist = 0.05, middleOfPatternWeight = 2f, tapGapMinMs = 310, tapGapMaxMs = 580,
            tapGapPastComfortMs = 120, hesitationChance = 0.1, hesitationMs = 380, addStyle = AddStyle.AvoidEasyRuns, saveTwistUntilLength = 8,
        };

        public static ComputerLevel Hard() => new ComputerLevel
        {
            label = "HARD", comfortableLength = 12, slipChanceWhenComfortable = 0.003, slipGrowthPastComfort = 0.02,
            slipChanceUnderTwist = 0.03, middleOfPatternWeight = 1.8, tapGapMinMs = 280, tapGapMaxMs = 520,
            tapGapPastComfortMs = 100, hesitationChance = 0.08, hesitationMs = 340, addStyle = AddStyle.Tricky, saveTwistUntilLength = 11,
        };
    }
}
