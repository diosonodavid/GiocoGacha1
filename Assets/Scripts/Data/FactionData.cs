using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class FactionRank
    {
        public string rankName;
        public int requiredReputationPoints;
        public int goldReward;
        public int gemsReward;
    }

    [CreateAssetMenu(fileName = "FactionData", menuName = "GachaGame/Data/Faction Data")]
    public class FactionData : ScriptableObject
    {
        public string factionId;
        public string factionName;
        [TextArea] public string description;
        public List<FactionRank> ranks = new();
    }
}
