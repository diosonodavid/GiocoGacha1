using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class ArenaSeasonReward
    {
        public CurrencyType currencyType;
        public int amount;
        public ItemData item;
    }

    // One rung of the PvP league ladder (Bronze/Silver/Gold/...); ArenaManager picks the highest
    // rank whose minPoints the player's current Arena Points satisfy.
    [CreateAssetMenu(fileName = "ArenaRankData", menuName = "GachaGame/Data/Arena Rank Data")]
    public class ArenaRankData : ScriptableObject
    {
        public string rankId;
        public string rankName;
        public int minPoints;
        public Sprite rankIcon;
        public List<ArenaSeasonReward> seasonEndRewards = new();
    }
}
