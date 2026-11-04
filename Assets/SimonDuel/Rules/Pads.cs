namespace SimonDuel.Rules
{
    // The four pads. A pad keeps its identity wherever it sits on the board.
    public enum PadId { Circle, Triangle, Square, Star }

    public static class Pads
    {
        public static readonly PadId[] All = { PadId.Circle, PadId.Triangle, PadId.Square, PadId.Star };
    }
}
