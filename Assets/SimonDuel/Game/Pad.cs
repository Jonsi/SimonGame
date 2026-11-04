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
        int slot;
        Vector2 slideFrom;
        float slideStart;
        float slideSeconds;

        public PadId Id => id;
        public int Slot => slot;
        public int HomeSlot => (int)id;

        public void Setup(Feel settings)
        {
            feel = settings;
            look = feel.looks[(int)id];

            // Home slots, left to right and top to bottom: Circle, Triangle, Square, Star.
            PlaceInSlot(HomeSlot);
            var rect = (RectTransform)transform;
            rect.anchoredPosition = center;
            rect.sizeDelta = feel.padSize;
            depth.rectTransform.anchoredPosition = new Vector2(0f, -feel.restingDepth);
            depth.color = look.depth;
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

        // Moves to another slot along an arc. The pad keeps its identity; only its place changes.
        public void SlideToSlot(int newSlot, float seconds)
        {
            slideFrom = ((RectTransform)transform).anchoredPosition;
            slideStart = Time.time;
            slideSeconds = seconds;
            PlaceInSlot(newSlot);
        }

        void PlaceInSlot(int newSlot)
        {
            slot = newSlot;
            float signX = slot % 2 == 0 ? -1f : 1f;
            float signY = slot < 2 ? 1f : -1f;
            center = new Vector2(signX * (feel.padSize.x + feel.padGap) / 2f, signY * (feel.padSize.y + feel.padGap) / 2f);

            // The shape is drawn for the top-left slot; mirroring puts its tight corner toward the board's center.
            var mirror = new Vector3(signX < 0 ? 1f : -1f, signY > 0 ? 1f : -1f, 1f);
            shape.rectTransform.localScale = mirror;
            depth.rectTransform.localScale = mirror;
            rim.rectTransform.localScale = mirror;
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
            Play(seconds, Color.white);
        }

        public void Play(float seconds, Color rimTint)
        {
            Light(seconds, feel.playbackScale, 0f, 0f, rimTint);
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
            Slide();
        }

        void Slide()
        {
            var rect = (RectTransform)transform;
            if (rect.anchoredPosition == center) return;

            float progress = Mathf.SmoothStep(0f, 1f, (Time.time - slideStart) / slideSeconds);
            float angle = Mathf.LerpAngle(Mathf.Atan2(slideFrom.y, slideFrom.x) * Mathf.Rad2Deg, Mathf.Atan2(center.y, center.x) * Mathf.Rad2Deg, progress) * Mathf.Deg2Rad;
            float radius = Mathf.Lerp(slideFrom.magnitude, center.magnitude, progress);
            rect.anchoredPosition = progress >= 1f ? center : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
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
