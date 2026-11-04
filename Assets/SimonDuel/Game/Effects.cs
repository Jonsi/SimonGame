using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimonDuel.Game
{
    // Small flying pieces: a burst from a pad, confetti for a win. Skipped under "Reduce motion".
    public class Effects : MonoBehaviour
    {
        [SerializeField] Feel feel;
        [SerializeField] Sprite piece;

        class Flying
        {
            public Image image;
            public Vector2 velocity;
            public float life, lifeLeft, gravity;
            public Color color;
        }

        readonly List<Flying> pool = new List<Flying>();

        void Awake()
        {
            for (int i = 0; i < feel.effectPieces; i++)
            {
                var go = new GameObject("Piece", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                var image = go.GetComponent<Image>();
                image.sprite = piece;
                image.raycastTarget = false;
                go.SetActive(false);
                pool.Add(new Flying { image = image });
            }
        }

        public void Burst(Vector3 worldPosition, Color color, int count)
        {
            if (GameSettings.ReduceMotion) return;

            Vector2 from = transform.InverseTransformPoint(worldPosition);
            for (int i = 0; i < count; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float speed = feel.burstSpeed * Random.Range(0.5f, 1f);
                Launch(from, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed, color, feel.burstSeconds, 0f, Random.Range(8f, 16f));
            }
        }

        public void Confetti(Color[] colors)
        {
            if (GameSettings.ReduceMotion) return;

            var area = ((RectTransform)transform).rect;
            for (int i = 0; i < feel.confettiPieces; i++)
            {
                var from = new Vector2(Random.Range(area.xMin, area.xMax), area.yMax + Random.Range(0f, 200f));
                var velocity = new Vector2(Random.Range(-60f, 60f), Random.Range(-120f, -30f));
                Launch(from, velocity, colors[i % colors.Length], feel.confettiSeconds * Random.Range(0.6f, 1f), feel.confettiGravity, Random.Range(10f, 20f));
            }
        }

        void Launch(Vector2 from, Vector2 velocity, Color color, float life, float gravity, float size)
        {
            var flying = pool.Find(f => f.lifeLeft <= 0f);
            if (flying == null) return;

            flying.velocity = velocity;
            flying.life = flying.lifeLeft = life;
            flying.gravity = gravity;
            flying.color = color;
            var rect = flying.image.rectTransform;
            rect.anchoredPosition = from;
            rect.sizeDelta = Vector2.one * size;
            flying.image.gameObject.SetActive(true);
        }

        void Update()
        {
            foreach (var flying in pool)
            {
                if (flying.lifeLeft <= 0f) continue;

                flying.lifeLeft -= Time.unscaledDeltaTime;
                flying.velocity.y -= flying.gravity * Time.unscaledDeltaTime;
                flying.image.rectTransform.anchoredPosition += flying.velocity * Time.unscaledDeltaTime;

                var color = flying.color;
                color.a = Mathf.Clamp01(flying.lifeLeft / flying.life * 2f);
                flying.image.color = color;
                if (flying.lifeLeft <= 0f) flying.image.gameObject.SetActive(false);
            }
        }
    }
}
