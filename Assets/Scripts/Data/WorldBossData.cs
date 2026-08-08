using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class WorldBossPhase
    {
        public string phaseName;
        [Range(0f, 1f)] public float hpThresholdPercent; // phase begins once remaining HP falls at/below this fraction
        public string attackPatternId;
    }

    [Serializable]
    public class WorldBossRewardTier
    {
        public int minRank;
        public int maxRank;
        public int goldReward;
        public int gemReward;
    }

    [CreateAssetMenu(fileName = "WorldBossData", menuName = "GachaGame/Data/World Boss Data")]
    public class WorldBossData : ScriptableObject
    {
        public string bossId;
        public string bossName;
        public long totalHP;
        public List<WorldBossPhase> phaseDataList = new();
        public List<WorldBossRewardTier> rewardTiers = new();
    }
}
