using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "TalentNodeData", menuName = "GachaGame/Data/Talent Node Data")]
    public class TalentNodeData : ScriptableObject
    {
        public string nodeId;
        public string nodeName;
        public GearStat statBonus;
        public float statMultiplier = 1f;
        public string unlockedSkillId;
        public List<TalentNodeData> prerequisites = new();
    }
}
