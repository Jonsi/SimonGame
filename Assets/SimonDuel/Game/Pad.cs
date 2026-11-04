using SimonDuel.Rules;
using UnityEngine;
using UnityEngine.UI;

namespace SimonDuel.Game
{
    // One pad on the board: where it sits, and its light, squash and tone when pressed.
    public class Pad : MonoBehaviour
    {
        [SerializeField] PadId id;
        [SerializeField] RectTransform face;
        [SerializeField] Image shape;
        [SerializeField] Image depth;
        [SerializeField] Image gloss;
        [SerializeField] Image rim;
        [SerializeField] Image symbol;
        [SerializeField] AudioSource tone;

        Feel feel;
        PadLook look;
        Vector2 center;
        float pressedAt = float.NegativeInfinity;
        float lightSeconds;
        float peakScale;
        float depthChange;
        float shakePixels;
        Color rimColor = Color.white;

        public PadId Id => id;

        public void Setup(Feel settings)
        {
            feel = settings;
            look = feel.looks[(int)id];

            // Home slots, left to right and top to bottom: Circle, Triangle, Square, Star.
            int slot = (int)id;
            float signX = slot % 2 == 0 ? -1f : 1f;
            float signY = slot < 2 ? 1f : -1f;
            center = new Vector2(signX * (feel.padSize.x + feel.padGap) / 2f, signY * (feel.padSize.y + feel.padGap) / 2f);

            var rect = (RectTransform)transform;
            rect.anchoredPosition = center;
            rect.sizeDelta = feel.padSize;
            // The shape is drawn for the top-left slot; mirroring puts its tight corner toward the board's center.
            var mirror = new Vector3(signX < 0 ? 1f : -1f, signY > 0 ? 1f : -1f, 1f);
            shape.rectTransform.localScale = mirror;
            depth.rectTransform.localScale = mirror;
            depth.rectTransform.anchoredPosition = new Vector2(0f, -feel.restingDepth);
            depth.color = look.depth;
            rim.rectTransform.localScale = mirror;
            rim.rectTransform.offsetMin = Vector2.one * -feel.rimOutset;
            rim.rectTransform.offsetMax = Vector2.one * feel.rimOutset;
            gloss.rectTransform.anchoredPosition = new Vector2(0f, -feel.glossInset);
            gloss.color = new Color(1f, 1f, 1f, feel.glossOpacity);
            symbol.color = feel.symbolColor;
            symbol.rectTransform.sizeDelta = Vector2.one * feel.symbolSize;
            lightSeconds = feel.lightSettleSeconds;
            peakScale = feel.pressedScale;
            Show(float.PositiveInfinity);
        }

        public bool Contains(Vector2 boardPoint)
        {
            var offset = boardPoint - center;
            return Mathf.Abs(offset.x) <= feel.padHitSize.x / 2f && Mathf.Abs(offset.y) <= feel.padHitSize.y / 2f;
        }

        // A player's press: dips into the board.
        public void Press()
        {
            Light(feel.lightSettleSeconds, feel.pressedScale, feel.restingDepth - feel.pressedDepth, 0f, Color.white);
        }

        // One step of the pattern, or the step that should have been pressed.
        public void Play(float seconds)
        {
            Light(seconds, feel.playbackScale, 0f, 0f, Color.white);
        }

        // A press on the wrong pad.
        public void ShowWrong()
        {
            Light(feel.wrongShakeSeconds, feel.pressedScale, feel.restingDepth - feel.pressedDepth, feel.wrongShakePixels, feel.wrongRimColor);
        }

        void Light(float seconds, float scale, float depthDrop, float shake, Color rimTint)
        {
            lightSeconds = seconds;
            peakScale = scale;
            depthChange = depthDrop;
            shakePixels = shake;
            rimColor = rimTint;
            pressedAt = Time.unscaledTime;
            tone.Stop();
            tone.Play();
            Show(0f);
        }

        void Update()
        {
            Show(Time.unscaledTime - pressedAt);
        }

        void Show(float sincePress)
        {
            float squash = sincePress < feel.squashInSeconds
                ? 1f - Mathf.Pow(1f - sincePress / feel.squashInSeconds, 2f)
                : 1f - Mathf.Clamp01((sincePress - feel.squashInSeconds) / feel.squashOutSeconds);

            float shake = 0f;
            if (sincePress < feel.wrongShakeSeconds)
            {
                float progress = sincePress / feel.wrongShakeSeconds;
                shake = shakePixels * (1f - progress) * Mathf.Sin(progress * feel.wrongShakeTurns * 2f * Mathf.PI);
            }

            transform.localScale = Vector3.one * Mathf.Lerp(1f, peakScale, squash);
            face.anchoredPosition = new Vector2(shake, -squash * depthChange);
            float settled = Mathf.SmoothStep(0f, 1f, sincePress / lightSeconds);
            shape.color = Color.Lerp(look.lit, look.rest, settled);
            rim.color = new Color(rimColor.r, rimColor.g, rimColor.b, 1f - settled);
        }
    }
}
