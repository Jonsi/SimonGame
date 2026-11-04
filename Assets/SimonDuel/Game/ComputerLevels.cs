using SimonDuel.Rules;
using UnityEngine;

namespace SimonDuel.Game
{
    // The numbers behind Easy, Medium and Hard.
    [CreateAssetMenu(menuName = "CuzGames/Computer Levels")]
    public class ComputerLevels : ScriptableObject
    {
        public ComputerLevel[] levels = { ComputerLevel.Easy(), ComputerLevel.Medium(), ComputerLevel.Hard() };
    }
}
