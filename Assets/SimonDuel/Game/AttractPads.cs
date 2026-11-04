using UnityEngine;
using UnityEngine.UI;

namespace SimonDuel.Game
{
    // The little boards on the menu light up one pad at a time, so the screen is never still.
    public class AttractPads : MonoBehaviour
    {
        [SerializeField] Feel feel;
        [SerializeField] Image[] pads;
        [SerializeField] float startDelay;

        float clock;
        int lit;

        void OnEnable()
        {
            clock = -startDelay;
            ShowAtRest();
        }

        void Update()
        {
            if (GameSettings.ReduceMotion) return;

            clock += Time.unscaledDeltaTime;
            if (clock < feel.attractStepSeconds) return;

            clock = 0f;
            ShowAtRest();
            lit = Random.Range(0, pads.Length);
            pads[lit].color = feel.looks[lit].lit;
        }

        void ShowAtRest()
        {
            for (int i = 0; i < pads.Length; i++) pads[i].color = feel.looks[i].rest;
        }
    }
}
