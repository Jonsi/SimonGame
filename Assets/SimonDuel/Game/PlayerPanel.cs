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

        public void Show(int score, bool active, Color color, Color idle)
        {
            outline.color = active ? color : idle;
            yourTurn.SetActive(active);
            for (int i = 0; i < pips.Length; i++)
                pips[i].color = i < score ? color : idle;
        }
    }
}
