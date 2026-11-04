using UnityEngine;
using UnityEngine.UI;

namespace SimonDuel.Game
{
    // One choice on the menu.
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour
    {
        [SerializeField] PlaySetup setup;

        public PlaySetup Setup => setup;
        public Button Button => GetComponent<Button>();
    }
}
