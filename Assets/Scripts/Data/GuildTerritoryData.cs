using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "GuildTerritoryData", menuName = "GachaGame/Data/Guild Territory Data")]
    public class GuildTerritoryData : ScriptableObject
    {
        public string territoryId;
        public string territoryName;
        public int controlBonusGoldPerHour;
        public int baseDefensePower;
        public List<string> adjacentTerritoryIds = new();
    }
}
