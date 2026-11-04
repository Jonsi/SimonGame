using System;

namespace SimonDuel.Game
{
    public enum GameMode { TwoPlayers, Computer, Practice }

    public enum PracticeKind { Classic, Endless, Twists, SightOnly, SoundOnly }

    // What the player picked on the menu.
    [Serializable]
    public struct PlaySetup
    {
        public GameMode mode;
        public int computerLevel;
        public PracticeKind practice;
    }
}
