using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "LeaderSkillData", menuName = "GachaGame/Data/Leader Skill Data")]
    public class LeaderSkillData : ScriptableObject
    {
        public string skillId;
        public string skillName;
        [TextArea] public string description;

        // Reuses StatModifier for its stat/value/isPercentage shape; durationTurns is ignored
        // here since a leader bonus lasts the whole battle rather than a fixed number of turns.
        public List<StatModifier> statBonuses = new();
    }
}
