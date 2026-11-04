namespace SimonDuel.Rules
{
    // The four pads. A pad keeps its identity wherever it sits on the board.
    public enum PadId { Circle, Triangle, Square, Star }

    // A one-turn rule change aimed at the other player.
    public enum TwistId { None, Reverse, Shift, Echo }

    public static class Pads
    {
        public static readonly PadId[] All = { PadId.Circle, PadId.Triangle, PadId.Square, PadId.Star };
    }
}
