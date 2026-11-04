using System;
using System.Collections;
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
        [SerializeField] Text rematchLabel;
        [SerializeField] GameObject menu;
        [SerializeField] MenuButton[] menuButtons;
        [SerializeField] Button openSettings;
        [SerializeField] GameObject settingsCard;
        [SerializeField] Button pause;
        [SerializeField] Button resume;
        [SerializeField] Button quit;
        [SerializeField] Button howToPlay;

        static readonly Color Idle = new Color32(0x2A, 0x52, 0x7D, 0xFF);

        public void Listen(Action onRematch, Action onHome, Action onNotes)
        {
            notes.onClick.AddListener(() => onNotes());
            rematch.onClick.AddListener(() => onRematch());
            home.onClick.AddListener(() => onHome());
        }

        public void ListenToMenu(Action<PlaySetup> onChoose)
        {
            foreach (var button in menuButtons)
            {
                var choice = button;
                button.Button.onClick.AddListener(() => onChoose(choice.Setup));
            }
        }

        // Newcomers are pointed at "How to play".
        public void ShowFirstTimeHint(bool show)
        {
            howToPlay.GetComponent<Image>().color = show ? feel.playerColors[0] : Idle;
            howToPlay.GetComponentInChildren<Text>().color = show ? feel.symbolColor : Color.white;
            howToPlay.GetComponentInChildren<Text>().text = show ? "NEW? HOW TO PLAY" : "HOW TO PLAY";
        }

        public void ShowMenu(bool show)
        {
            menu.SetActive(show);
        }

        public void ListenToPause(Action onPause, Action onResume, Action onQuit, Action onHowToPlay)
        {
            howToPlay.onClick.AddListener(() => onHowToPlay());
            pause.onClick.AddListener(() => onPause());
            resume.onClick.AddListener(() => onResume());
            quit.onClick.AddListener(() => onQuit());
            openSettings.onClick.AddListener(() => settingsCard.SetActive(true));
        }

        public bool IsOverPauseButton(Vector2 screenPoint)
        {
            return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)pause.transform, screenPoint);
        }

        public void ShowRingError()
        {
            timerRing.color = feel.wrongRimColor;
            timerRing.fillAmount = 1f;
        }

        public void ShowNames(string[] names, string secondNumber, bool showSecondPlayer)
        {
            foreach (var player in players) player.ResetScore();
            players[0].ShowName(names[0], "1");
            players[1].ShowName(names[1], secondNumber);
            players[0].gameObject.SetActive(true);
            players[1].gameObject.SetActive(showSecondPlayer);
            players[0].ShowTwistArea(showSecondPlayer);
        }

        public void ShowTutorialLook()
        {
            foreach (var player in players) player.gameObject.SetActive(false);
            length.text = "";
            bannerLine.color = feel.playerColors[0];
            timerRing.color = feel.playerColors[0];
            timerRing.fillClockwise = true;
            timerRing.fillAmount = 1f;
        }

        public void ListenToTwists(Action onUse, Action onKeep)
        {
            foreach (var player in players) player.Listen(onUse, onKeep);
        }

        public void ShowPlayers(int activePlayer, Referee referee, int offeredTo = -1)
        {
            for (int i = 0; i < players.Length; i++)
            {
                players[i].Show(referee.Score(i), activePlayer == i, feel.playerColors[i], Idle, feel);
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

        public void ShowResult(string title, int colorPlayer, string score, string againLabel, int notesSaved)
        {
            resultCover.SetActive(true);
            resultTitle.text = title;
            resultTitle.color = feel.playerColors[colorPlayer];
            resultScore.text = score;
            rematchLabel.text = againLabel;
            notes.gameObject.SetActive(notesSaved > 0);
            notesLabel.text = $"Download match notes ({notesSaved})";
            StartCoroutine(RevealResult());
        }

        IEnumerator RevealResult()
        {
            var card = resultCover.GetComponent<CanvasGroup>();
            var rect = (RectTransform)resultCover.transform;
            float seconds = GameSettings.ReduceMotion ? 0f : feel.resultSeconds;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                float progress = t / seconds;
                card.alpha = progress;
                rect.localScale = Vector3.one * Mathf.Lerp(feel.resultStartScale, 1f, progress);
                yield return null;
            }
            card.alpha = 1f;
            rect.localScale = Vector3.one;
        }

        public void HideResult()
        {
            resultCover.SetActive(false);
        }
    }
}
