using System;
using System.Collections;
using SimonDuel.Rules;
using UnityEngine;
using UnityEngine.UI;

namespace SimonDuel.Game
{
    // One player's side of the screen: score pips and whether it is their turn.
    public class PlayerPanel : MonoBehaviour
    {
        [SerializeField] Image outline;
        [SerializeField] Image[] pips;
        [SerializeField] GameObject yourTurn;
        [SerializeField] Image[] chargePips;
        [SerializeField] Button twistCard;
        [SerializeField] Text twistLabel;
        [SerializeField] Button keep;
        [SerializeField] Text nameLabel;
        [SerializeField] Text numberLabel;

        public void ShowName(string playerName, string number)
        {
            nameLabel.text = playerName;
            numberLabel.text = number;
        }

        public void Listen(Action onUse, Action onKeep)
        {
            twistCard.onClick.AddListener(() => onUse());
            keep.onClick.AddListener(() => onKeep());
        }

        public void ShowTwistArea(bool show)
        {
            twistCard.gameObject.SetActive(show);
            foreach (var pip in chargePips) pip.gameObject.SetActive(show);
        }

        public void ShowTwist(int charge, TwistId held, Color twistColor, Color idle, bool offered)
        {
            for (int i = 0; i < chargePips.Length; i++)
                chargePips[i].color = i < charge ? twistColor : idle;

            string name = held.ToString().ToUpperInvariant();
            twistLabel.text = held == TwistId.None ? "NO TWIST" : offered ? $"USE {name}" : name;
            twistCard.interactable = offered;
            keep.gameObject.SetActive(offered);
        }

        int shownScore;

        public void ResetScore()
        {
            shownScore = 0;
        }

        public void Show(int score, bool active, Color color, Color idle, Feel feel)
        {
            if (score > shownScore && gameObject.activeInHierarchy && !GameSettings.ReduceMotion)
                StartCoroutine(PopPip(pips[score - 1].rectTransform, feel));
            shownScore = score;

            outline.color = active ? color : idle;
            yourTurn.SetActive(active);
            for (int i = 0; i < pips.Length; i++)
                pips[i].color = i < score ? color : idle;
        }

        IEnumerator PopPip(RectTransform pip, Feel feel)
        {
            for (float t = 0f; t < feel.pipPopSeconds; t += Time.unscaledDeltaTime)
            {
                float progress = t / feel.pipPopSeconds;
                pip.localScale = Vector3.one * Mathf.Lerp(feel.pipPopScale, 1f, progress * progress);
                yield return null;
            }
            pip.localScale = Vector3.one;
        }
    }
}
