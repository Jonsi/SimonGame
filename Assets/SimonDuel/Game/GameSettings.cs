using UnityEngine;

namespace SimonDuel.Game
{
    // The player's choices, kept in this browser.
    public static class GameSettings
    {
        const string SoundKey = "CuzGames.Sound";
        const string ReduceMotionKey = "CuzGames.ReduceMotion";
        const string SlowPlaybackKey = "CuzGames.SlowPlayback";

        public static bool Sound
        {
            get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
            set { Save(SoundKey, value); Apply(); }
        }

        public static bool ReduceMotion
        {
            get => PlayerPrefs.GetInt(ReduceMotionKey, 0) == 1;
            set => Save(ReduceMotionKey, value);
        }

        public static bool SlowPlayback
        {
            get => PlayerPrefs.GetInt(SlowPlaybackKey, 0) == 1;
            set => Save(SlowPlaybackKey, value);
        }

        public static void Apply()
        {
            AudioListener.volume = Sound ? 1f : 0f;
        }

        static void Save(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
