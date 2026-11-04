using System;
using SimonDuel.Rules;
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
        [SerializeField] Button notes;
        [SerializeField] Text notesLabel;

        static readonly Color Idle = new Color32(0x2A, 0x52, 0x7D, 0xFF);

        public void Listen(Action onRematch, Action onHome, Action onNotes)
        {
            notes.onClick.AddListener(() => onNotes());
            rematch.onClick.AddListener(() => onRematch());
            home.onClick.AddListener(() => onHome());
        }

        public void ListenToTwists(Action onUse, Action onKeep)
        {
            foreach (var player in players) player.Listen(onUse, onKeep);
        }

        public void ShowPlayers(int activePlayer, Referee referee, int offeredTo = -1)
        {
            for (int i = 0; i < players.Length; i++)
            {
                players[i].Show(referee.Score(i), activePlayer == i, feel.playerColors[i], Idle);
                players[i].ShowTwist(referee.Charge(i), referee.HeldTwist(i), feel.twistColor, Idle, offeredTo == i);
            }
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

        public void ShowTimerDirection(bool backwards)
        {
            timerRing.fillClockwise = !backwards;
        }

        public void ShowTimer(float fractionLeft)
        {
            timerRing.fillAmount = fractionLeft;
        }

        public void ShowPaused(bool paused)
        {
            pausedCover.SetActive(paused);
        }

        public void ShowResult(int winner, int score0, int score1, int notesSaved)
        {
            notesLabel.text = $"Download match notes ({notesSaved})";
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
