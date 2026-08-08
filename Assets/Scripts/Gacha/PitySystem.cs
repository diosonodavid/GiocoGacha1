using System;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Utils;

namespace GachaGame.Gacha
{
    // Thin, banner-scoped facade over GachaPityState: GachaSystem mutates State directly as each
    // pull resolves (GachaPityState's own contract is "pass the same instance back in across
    // pulls"), so this class exists to expose that progress in reader-friendly terms rather than
    // to duplicate the soft/hard pity math GachaSystem already owns and has tests against.
    public class PitySystem
    {
        public GachaPityState State { get; } = new();

        public int PullsSinceLastFiveStar => State.pullsSinceLastFiveStar;
        public bool IsFiftyFiftyGuaranteed => State.isFiftyFiftyGuaranteed;
        public int PullsUntilGuarantee => Math.Max(0, GameConstants.HardPityThreshold - State.pullsSinceLastFiveStar);
        public bool IsFiveStarGuaranteedNextPull => PullsUntilGuarantee <= 0;

        public float GetCurrentFiveStarRate() => GachaSystem.GetFiveStarRate(State.pullsSinceLastFiveStar + 1);

        public void Reset()
        {
            State.pullsSinceLastFiveStar = 0;
            State.isFiftyFiftyGuaranteed = false;
        }
    }
}
