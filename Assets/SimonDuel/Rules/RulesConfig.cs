using System;
using System.Collections.Generic;

namespace SimonDuel.Rules
{
    // Every rule number. All times are whole milliseconds.
    [Serializable]
    public sealed class RulesConfig
    {
        public int ralliesToWin = 2;
        public int startingSteps = 2;

        public int timerBaseMs = 3000;
        public int timerPerStepMs = 750;
        public int timerMaxMs = 14000;

        public int playbackLightMs = 420;
        public int playbackLightFloorMs = 320;
        public int playbackLightDropPerStepMs = 10;
        public int playbackGapMs = 140;
        public int playbackGapFloorMs = 100;
        public int playbackGapDropPerStepMs = 4;

        public int chargeToEarnTwist = 3;
        public int turnsBeforeTwists = 2;
        public bool chargeResetsEachRally = true;
        public bool twistsArmThemselves = false;
        public List<TwistId> twistBag = new List<TwistId> { TwistId.Reverse, TwistId.Shift, TwistId.Echo };
    }
}
