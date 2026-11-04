using UnityEngine;

namespace SimonDuel.Game
{
    // How long each moment between turns lasts (GDD §6).
    [CreateAssetMenu(menuName = "CuzGames/Timing")]
    public class Timing : ScriptableObject
    {
        public float ownershipSeconds = 0.55f;
        public float readyBeatSeconds = 0.35f;
        public float addOneHoldSeconds = 0.42f;
        public float rallyLossRevealSeconds = 1.10f;
        public float pointCelebrationSeconds = 1.40f;
    }
}
