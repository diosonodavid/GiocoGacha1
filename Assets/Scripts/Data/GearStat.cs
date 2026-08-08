using System;

namespace GachaGame.Data
{
    [Serializable]
    public class GearStat
    {
        public StatType statType;
        public float value;
        public bool isPercentage;
    }
}
