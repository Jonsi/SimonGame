using System;
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

        public void Listen(Action onUse, Action onKeep)
        {
            twistCard.onClick.AddListener(() => onUse());
            keep.onClick.AddListener(() => onKeep());
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

        public void Show(int score, bool active, Color color, Color idle)
        {
            outline.color = active ? color : idle;
            yourTurn.SetActive(active);
            for (int i = 0; i < pips.Length; i++)
                pips[i].color = i < score ? color : idle;
        }
    }
}
