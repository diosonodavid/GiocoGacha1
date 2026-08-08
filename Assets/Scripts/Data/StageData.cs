using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class StageReward
    {
        public CurrencyType currencyType;
        public int amount;
        public ItemData item;
    }

    [CreateAssetMenu(fileName = "StageData", menuName = "GachaGame/Data/Stage Data")]
    public class StageData : ScriptableObject
    {
        public string stageId;
        public string stageName;
        public int staminaCost;
        public int recommendedLevel;
        public List<CharacterBaseData> listNemici = new();
        public List<StageReward> firstClearRewards = new();
    }
}
