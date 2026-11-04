using SimonDuel.Rules;
using UnityEngine;

namespace SimonDuel.Game
{
    // The rule numbers as a settings asset. The rules code itself stays free of Unity.
    [CreateAssetMenu(menuName = "CuzGames/Duel Rules")]
    public class DuelRules : ScriptableObject
    {
        public RulesConfig rules = new RulesConfig();
    }
}
