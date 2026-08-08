using System;

namespace GachaGame.Data
{
    [Serializable]
    public class ShopPackageData
    {
        public string packageId;
        public CurrencyType costCurrency;
        public int cost;
        public CurrencyType rewardCurrency;
        public int rewardAmount;
        public int purchaseLimit; // 0 = unlimited
        public ShopLimitPeriod limitPeriod;
    }
}
