using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "TowerFloorData", menuName = "GachaGame/Data/Tower Floor Data")]
    public class TowerFloorData : ScriptableObject
    {
        public int floorNumber;
        public List<CharacterBaseData> enemyTeam = new();
        public int staminaCost;
        public List<ItemData> rewardItems = new();
    }
}
