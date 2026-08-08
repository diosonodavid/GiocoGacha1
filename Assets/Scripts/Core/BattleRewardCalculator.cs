using System.Collections.Generic;
using GachaGame.Data;

namespace GachaGame.Core
{
    public struct BattleRewards
    {
        public int gold;
        public int accountExp;
        public List<ItemData> itemDrops;
    }

    public static class BattleRewardCalculator
    {
        public static BattleRewards CalculateRewards(
            IEnumerable<CharacterInstance> defeatedEnemies,
            int goldPerEnemy,
            int accountExpPerEnemy,
            IEnumerable<ItemData> possibleDrops = null,
            float dropChance = 0.3f,
            System.Random random = null)
        {
            random ??= new System.Random();

            int defeatedCount = 0;
            foreach (var _ in defeatedEnemies ?? System.Array.Empty<CharacterInstance>())
                defeatedCount++;

            var rewards = new BattleRewards
            {
                gold = goldPerEnemy * defeatedCount,
                accountExp = accountExpPerEnemy * defeatedCount,
                itemDrops = new List<ItemData>()
            };

            if (possibleDrops != null)
            {
                foreach (var item in possibleDrops)
                {
                    if (item != null && random.NextDouble() < dropChance)
                        rewards.itemDrops.Add(item);
                }
            }

            return rewards;
        }
    }
}
