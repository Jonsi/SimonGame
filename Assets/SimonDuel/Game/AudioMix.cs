using UnityEngine;

namespace SimonDuel.Game
{
    // How loud each group of sounds is.
    [CreateAssetMenu(menuName = "CuzGames/Audio Mix")]
    public class AudioMix : ScriptableObject
    {
        [Range(0f, 1f)] public float pads = 0.9f;
        [Range(0f, 1f)] public float effects = 0.7f;
    }
}
