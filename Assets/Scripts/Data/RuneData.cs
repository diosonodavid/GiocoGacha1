using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    // Instances are created at runtime via ScriptableObject.CreateInstance<RuneData>() when a rune
    // drops (battle reward or gacha), not authored as pre-existing project assets - each dropped
    // rune is its own object, so refining or re-rolling one never mutates a shared definition or
    // another player's copy. setId links it to an EquipmentSetData for 2pc/4pc bonuses.
    [CreateAssetMenu(fileName = "RuneData", menuName = "GachaGame/Data/Rune Data")]
    public class RuneData : ItemData
    {
        public string setId;
        public int slotIndex; // 1-6, classic rune-slot layout
        public GearStat mainStat;
        public List<GearStat> subStats = new();
        public int refinementLevel;
    }
}
