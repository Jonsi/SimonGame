namespace SimonDuel.Rules
{
    // The match's only source of chance: the same seed always gives the same numbers.
    public sealed class SeededRandom
    {
        ulong state;

        public SeededRandom(ulong seed)
        {
            state = seed;
        }

        public ulong NextValue()
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong z = state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public bool Chance(double probability)
        {
            return NextValue() % 1000000UL < (ulong)(probability * 1000000d);
        }

        public int Next(int limit)
        {
            return (int)(NextValue() % (ulong)limit);
        }
    }
}
