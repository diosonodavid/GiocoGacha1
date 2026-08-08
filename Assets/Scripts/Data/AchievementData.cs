using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class AchievementReward
    {
        public int gemsReward;
        public List<ItemData> itemRewards = new();
    }

    [CreateAssetMenu(fileName = "AchievementData", menuName = "GachaGame/Data/Achievement Data")]
    public class AchievementData : ScriptableObject
    {
        public string id;
        public string title;
        [TextArea] public string description;

        // Which game stat AchievementManager.ReportStat() routes to this trophy - needed so the
        // manager can auto-track progress from game events rather than requiring every call site
        // to know achievement ids directly.
        public AchievementStatType statType;
        public int targetValue;
        public AchievementReward reward = new();
    }
}
