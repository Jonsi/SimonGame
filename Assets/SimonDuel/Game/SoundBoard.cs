using UnityEngine;

namespace SimonDuel.Game
{
    public enum GameSound { Done, Wrong, Twist, Point, Win, TurnOne, TurnTwo, Click }

    // Every sound that is not a pad's own tone. Clips are in the order of GameSound.
    public class SoundBoard : MonoBehaviour
    {
        [SerializeField] AudioSource source;
        [SerializeField] AudioMix mix;
        [SerializeField] AudioClip[] clips;

        public void Play(GameSound sound)
        {
            source.PlayOneShot(clips[(int)sound], mix.effects);
        }
    }
}
