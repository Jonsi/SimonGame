using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimonDuel.Game
{
    // Everything on screen around the pads. Shows what it is told and decides nothing.
    public class DuelScreen : MonoBehaviour
    {
        [SerializeField] Feel feel;
        [SerializeField] Text banner;
        [SerializeField] Image bannerLine;
        [SerializeField] Text length;
        [SerializeField] Image timerRing;
        [SerializeField] PlayerPanel[] players;
        [SerializeField] GameObject pausedCover;
        [SerializeField] GameObject resultCover;
        [SerializeField] Text resultTitle;
        [SerializeField] Text resultScore;
        [SerializeField] Button rematch;
        [SerializeField] Button home;

        static readonly Color Idle = new Color32(0x2A, 0x52, 0x7D, 0xFF);

        public void Listen(Action onRematch, Action onHome)
        {
            rematch.onClick.AddListener(() => onRematch());
            home.onClick.AddListener(() => onHome());
        }

        public void ShowPlayers(int activePlayer, int score0, int score1)
        {
            players[0].Show(score0, activePlayer == 0, feel.playerColors[0], Idle);
            players[1].Show(score1, activePlayer == 1, feel.playerColors[1], Idle);
            var color = activePlayer >= 0 ? feel.playerColors[activePlayer] : Idle;
            bannerLine.color = color;
            timerRing.color = color;
        }

        public void ShowBanner(string text)
        {
            banner.text = text;
        }

        public void ShowLength(int steps)
        {
            length.text = $"LENGTH {steps}";
        }

        public void ShowTimer(float fractionLeft)
        {
            timerRing.fillAmount = fractionLeft;
        }

        public void ShowPaused(bool paused)
        {
            pausedCover.SetActive(paused);
        }

        public void ShowResult(int winner, int score0, int score1)
        {
            resultCover.SetActive(true);
            resultTitle.text = $"PLAYER {winner + 1} WINS";
            resultTitle.color = feel.playerColors[winner];
            resultScore.text = $"{score0} - {score1}";
        }

        public void HideResult()
        {
            resultCover.SetActive(false);
        }
    }
}
