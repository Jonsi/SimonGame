using System;
using UnityEngine;

namespace SimonDuel.Game
{
    // How the pads look and respond. Sizes are in pixels on the 1920 × 1080 widescreen layout.
    [CreateAssetMenu(menuName = "CuzGames/Feel")]
    public class Feel : ScriptableObject
    {
        [Header("Board")]
        public float boardDropFromCenter = 50f;
        public float ringDiameter = 800f;
        public Vector2 padSize = new Vector2(256f, 256f);
        public Vector2 padHitSize = new Vector2(280f, 280f);
        public float padGap = 24f;
        public float symbolSize = 108f;
        public float glossInset = 22f;
        public float glossOpacity = 0.16f;
        public float rimOutset = 8f;

        [Header("Press")]
        public float restingDepth = 12f;
        public float pressedDepth = 3f;
        public float pressedScale = 0.96f;
        public float squashInSeconds = 0.07f;
        public float squashOutSeconds = 0.11f;
        public float lightSettleSeconds = 0.42f;

        [Header("Colors, in pad order: Circle, Triangle, Square, Star")]
        public PadLook[] looks =
        {
            new PadLook { rest = new Color32(0xFF, 0x5A, 0x52, 0xFF), lit = new Color32(0xFF, 0x91, 0x88, 0xFF), depth = new Color32(0xB9, 0x2F, 0x3C, 0xFF) },
            new PadLook { rest = new Color32(0xFF, 0xC6, 0x2E, 0xFF), lit = new Color32(0xFF, 0xE4, 0x77, 0xFF), depth = new Color32(0xC8, 0x89, 0x00, 0xFF) },
            new PadLook { rest = new Color32(0x42, 0xD7, 0x9A, 0xFF), lit = new Color32(0x87, 0xEB, 0xC2, 0xFF), depth = new Color32(0x16, 0x8E, 0x67, 0xFF) },
            new PadLook { rest = new Color32(0x2B, 0x9D, 0xF4, 0xFF), lit = new Color32(0x79, 0xC9, 0xFF, 0xFF), depth = new Color32(0x12, 0x5F, 0xAF, 0xFF) },
        };
        public Color symbolColor = new Color32(0x07, 0x14, 0x2B, 0xFF);
    }

    [Serializable]
    public struct PadLook
    {
        public Color rest;
        public Color lit;
        public Color depth;
    }
}
