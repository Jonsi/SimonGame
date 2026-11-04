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

        [Header("Glow and ripple")]
        public float haloOutset = 46f;
        public float haloOpacity = 0.6f;
        public float rippleSeconds = 0.26f;
        public float rippleGrowth = 0.16f;
        public float rippleOpacity = 0.55f;

        [Header("Flying pieces")]
        public int effectPieces = 120;
        public int burstPieces = 12;
        public int wrongBurstPieces = 20;
        public float burstSpeed = 420f;
        public float burstSeconds = 0.45f;
        public int confettiPieces = 90;
        public float confettiSeconds = 3f;
        public float confettiGravity = 260f;

        [Header("Screens")]
        public float handOverSeconds = 0.45f;
        public float pipPopSeconds = 0.5f;
        public float pipPopScale = 1.7f;
        public float resultSeconds = 0.42f;
        public float resultStartScale = 0.92f;

        [Header("Pattern playback and slips")]
        public float playbackScale = 1.04f;
        public float wrongShakePixels = 16f;
        public float wrongShakeSeconds = 0.32f;
        public float wrongShakeTurns = 3f;
        public Color wrongRimColor = new Color32(0xFF, 0x46, 0x58, 0xFF);
        public Color twistColor = new Color32(0xA6, 0x64, 0xFF, 0xFF);

        [Header("Colors, in pad order: Circle, Triangle, Square, Star")]
        public PadLook[] looks =
        {
            new PadLook { rest = new Color32(0xFF, 0x5A, 0x52, 0xFF), lit = new Color32(0xFF, 0x91, 0x88, 0xFF), depth = new Color32(0xB9, 0x2F, 0x3C, 0xFF) },
            new PadLook { rest = new Color32(0xFF, 0xC6, 0x2E, 0xFF), lit = new Color32(0xFF, 0xE4, 0x77, 0xFF), depth = new Color32(0xC8, 0x89, 0x00, 0xFF) },
            new PadLook { rest = new Color32(0x42, 0xD7, 0x9A, 0xFF), lit = new Color32(0x87, 0xEB, 0xC2, 0xFF), depth = new Color32(0x16, 0x8E, 0x67, 0xFF) },
            new PadLook { rest = new Color32(0x2B, 0x9D, 0xF4, 0xFF), lit = new Color32(0x79, 0xC9, 0xFF, 0xFF), depth = new Color32(0x12, 0x5F, 0xAF, 0xFF) },
        };
        public Color symbolColor = new Color32(0x07, 0x14, 0x2B, 0xFF);
        public Color[] playerColors =
        {
            new Color32(0xFF, 0x94, 0x18, 0xFF),
            new Color32(0x20, 0xD7, 0xED, 0xFF),
        };
    }

    [Serializable]
    public struct PadLook
    {
        public Color rest;
        public Color lit;
        public Color depth;
    }
}
