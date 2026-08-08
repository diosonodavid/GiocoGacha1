using System;

namespace GachaGame.Data
{
    [Serializable]
    public struct StatModifier
    {
        public StatType stat;
        public float value;
        public bool isPercentage;
        public int durationTurns;
    }
}
