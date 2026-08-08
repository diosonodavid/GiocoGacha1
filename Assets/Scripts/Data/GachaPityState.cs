using System;

namespace GachaGame.Data
{
    // Mutable per-banner pity progress; pass the same instance back into GachaSystem across pulls.
    [Serializable]
    public class GachaPityState
    {
        public int pullsSinceLastFiveStar;
        public bool isFiftyFiftyGuaranteed;
    }
}
