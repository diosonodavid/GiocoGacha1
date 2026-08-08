using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "GuildBossData", menuName = "GachaGame/Data/Guild Boss Data")]
    public class GuildBossData : ScriptableObject
    {
        public string bossId;
        public string bossName;
        public long totalHp;
        public CharacterBaseData bossCharacter;
        public int recommendedLevel;
    }
}
