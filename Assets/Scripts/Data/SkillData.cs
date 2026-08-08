using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "GachaGame/Data/Skill Data")]
    public class SkillData : ScriptableObject
    {
        public string skillId;
        public string skillName;
        [TextArea] public string description;
        public int cooldown;
        public float damageMultiplier;
        public bool isAoE;
        public List<StatusEffectData> statusEffects = new();
    }
}
