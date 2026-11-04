using UnityEngine;
using UnityEngine.UI;

namespace SimonDuel.Game
{
    // The settings card: each button flips one choice and shows where it stands.
    public class SettingsCard : MonoBehaviour
    {
        [SerializeField] Button sound;
        [SerializeField] Button reduceMotion;
        [SerializeField] Button slowPlayback;
        [SerializeField] Button close;

        void Awake()
        {
            sound.onClick.AddListener(() => { GameSettings.Sound = !GameSettings.Sound; Refresh(); });
            reduceMotion.onClick.AddListener(() => { GameSettings.ReduceMotion = !GameSettings.ReduceMotion; Refresh(); });
            slowPlayback.onClick.AddListener(() => { GameSettings.SlowPlayback = !GameSettings.SlowPlayback; Refresh(); });
            close.onClick.AddListener(() => gameObject.SetActive(false));
        }

        void OnEnable()
        {
            Refresh();
        }

        void Refresh()
        {
            Label(sound, "SOUND", GameSettings.Sound);
            Label(reduceMotion, "REDUCE MOTION", GameSettings.ReduceMotion);
            Label(slowPlayback, "SLOWER PATTERNS", GameSettings.SlowPlayback);
        }

        static void Label(Button button, string name, bool on)
        {
            button.GetComponentInChildren<Text>().text = $"{name}: {(on ? "ON" : "OFF")}";
        }
    }
}
