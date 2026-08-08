using GachaGame.Data;
using GachaGame.Managers;

namespace GachaGame.Inventory
{
    // Levels up a single piece of gear by consuming an EXP-material item plus gold, reusing
    // InventoryManager's existing material bucket instead of introducing a second currency.
    public static class EquipmentEnhancer
    {
        public const int MaxEnhanceLevel = 15;
        public const string ExpMaterialItemId = "gear_exp_material";

        private const int ExpMaterialsPerLevel = 3;
        private const int GoldCostPerLevel = 500;

        public static int GetExpMaterialCost(int currentEnhanceLevel) => ExpMaterialsPerLevel * (currentEnhanceLevel + 1);

        public static int GetGoldCost(int currentEnhanceLevel) => GoldCostPerLevel * (currentEnhanceLevel + 1);

        public static bool TryEnhance(GearData gear, InventoryManager inventoryManager, CurrencyManager currencyManager)
        {
            if (gear == null || inventoryManager == null || currencyManager == null) return false;
            if (gear.enhanceLevel >= MaxEnhanceLevel) return false;

            int expCost = GetExpMaterialCost(gear.enhanceLevel);
            int goldCost = GetGoldCost(gear.enhanceLevel);

            if (!inventoryManager.TryConsumeMaterial(ExpMaterialItemId, expCost)) return false;

            if (!currencyManager.TrySpendCurrency(CurrencyType.Gold, goldCost))
            {
                inventoryManager.AddMaterial(ExpMaterialItemId, expCost); // refund since the gold spend failed
                return false;
            }

            gear.enhanceLevel++;
            return true;
        }
    }
}
