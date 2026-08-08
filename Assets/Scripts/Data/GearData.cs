using System;
using System.Collections.Generic;

namespace GachaGame.Data
{
    [Serializable]
    public class GearData
    {
        public string gearId;
        public GearSlotType slotType;
        public GearStat mainStat;
        public List<GearStat> subStats = new();
        public Rarity rarity;
        public int enhanceLevel;
    }
}
