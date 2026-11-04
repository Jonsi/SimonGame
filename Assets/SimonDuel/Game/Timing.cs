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

        [Header("Twists")]
        public float twistNameSeconds = 0.65f;
        public float twistFirstTimeSeconds = 1.25f;
        public float twistChoiceSeconds = 4f;
        public float shiftSeconds = 0.65f;
        public float echoGapSeconds = 0.08f;
        public float demoStepSeconds = 0.25f;
        public float slowPlaybackFactor = 1.4f;

        [Header("Tutorial")]
        public float tutorialLineSeconds = 1.9f;
        public float tutorialStepSeconds = 0.5f;
        public float tutorialGapSeconds = 0.18f;
        public float tutorialPauseSeconds = 0.5f;
    }
}
