using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Inventory
{
    // Owns a per-character shard ledger, distinct from InventoryManager's rarity-wide memory
    // fragments (which feed the Memory system's memoryLevel): shards are tied to one specific
    // character and feed CharacterAscensionSystem instead. Both are populated off the same
    // duplicate-pull event - GachaManager notifies this service in addition to, not instead of,
    // InventoryManager.ApplyPullResult, so neither system's bookkeeping changes.
    public class ShardManager : MonoBehaviour, IService
    {
        private const int ShardsPerDuplicateThreeStars = 5;
        private const int ShardsPerDuplicateFourStars = 10;
        private const int ShardsPerDuplicateFiveStars = 20;

        private readonly Dictionary<string, int> shardsByCharacter = new();

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(ShardManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void AddShards(string baseDataId, int amount)
        {
            if (string.IsNullOrEmpty(baseDataId) || amount <= 0) return;
            shardsByCharacter[baseDataId] = GetShardCount(baseDataId) + amount;
        }

        public bool TryConsumeShards(string baseDataId, int amount)
        {
            if (string.IsNullOrEmpty(baseDataId) || amount <= 0) return true;
            if (GetShardCount(baseDataId) < amount) return false;
            shardsByCharacter[baseDataId] -= amount;
            return true;
        }

        public int GetShardCount(string baseDataId) =>
            !string.IsNullOrEmpty(baseDataId) && shardsByCharacter.TryGetValue(baseDataId, out var count) ? count : 0;

        // Called by GachaManager right after InventoryManager.ApplyPullResult; only duplicates convert.
        public void HandlePullResult(GachaPullResult result)
        {
            if (result.pulledCharacter == null || result.isNew) return;

            int amount = result.rarity switch
            {
                Rarity.FiveStars => ShardsPerDuplicateFiveStars,
                Rarity.FourStars => ShardsPerDuplicateFourStars,
                _ => ShardsPerDuplicateThreeStars
            };

            AddShards(result.pulledCharacter.id, amount);
        }
    }
}
