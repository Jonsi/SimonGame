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
            Show(float.PositiveInfinity);
        }

        public bool Contains(Vector2 boardPoint)
        {
            var offset = boardPoint - center;
            return Mathf.Abs(offset.x) <= feel.padHitSize.x / 2f && Mathf.Abs(offset.y) <= feel.padHitSize.y / 2f;
        }

        public void Press()
        {
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

            transform.localScale = Vector3.one * Mathf.Lerp(1f, feel.pressedScale, squash);
            face.anchoredPosition = new Vector2(0f, -squash * (feel.restingDepth - feel.pressedDepth));
            float settled = Mathf.SmoothStep(0f, 1f, sincePress / feel.lightSettleSeconds);
            shape.color = Color.Lerp(look.lit, look.rest, settled);
            rim.color = new Color(1f, 1f, 1f, 1f - settled);
        }
    }
}
