using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "CharacterBaseData", menuName = "GachaGame/Data/Character Base Data")]
    public class CharacterBaseData : ScriptableObject
    {
        public string id;
        public string characterName;
        public Rarity rarity;
        public ElementType element;
        public SerializableDictionary<StatType, float> baseStats = new();
        public LeaderSkillData leaderSkill;
    }
}
