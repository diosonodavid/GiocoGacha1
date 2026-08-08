using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "EquipmentSetData", menuName = "GachaGame/Data/Equipment Set Data")]
    public class EquipmentSetData : ScriptableObject
    {
        public string setId;
        public string setName;
        public List<GearStat> twoPieceBonus = new();
        public List<GearStat> fourPieceBonus = new();
    }
}
