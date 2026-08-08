using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Inventory;
using GachaGame.Managers;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Gacha
{
    // Owns the actual summon transaction: spends gems, rolls rarities through the shared
    // GachaSystem (Core) so the probability math stays in one already-tested place, applies the
    // results to the player's inventory, and keeps pity progress (PitySystem) up to date as a
    // side effect of every pull rather than as a separate bookkeeping step.
    public class GachaManager : MonoBehaviour, IService
    {
        private const int TenPullCount = 10;

        [SerializeField] private BannerController bannerController;

        private GachaSystem gachaSystem;
        private CurrencyManager currencyManager;
        private InventoryManager inventoryManager;
        private ShardManager shardManager;

        public PitySystem Pity { get; private set; }
        public GachaHistory History { get; private set; }

        private void Awake()
        {
            gachaSystem = new GachaSystem();
            Pity = new PitySystem();
            History = new GachaHistory();
        }

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out currencyManager);
            ServiceLocator.Instance.TryGet(out inventoryManager);
            ServiceLocator.Instance.TryGet(out shardManager); // optional: ascension shards on duplicate pulls

            Debug.Log($"{nameof(GachaManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public float GetCurrentFiveStarRate() => Pity.GetCurrentFiveStarRate();

        public float GetCurrentFourStarRate()
        {
            float fiveStarRate = GetCurrentFiveStarRate();
            return Mathf.Max(0f, Mathf.Min(GameConstants.GachaFourStarBaseRate, 100f - fiveStarRate));
        }

        public float GetCurrentThreeStarRate() => Mathf.Max(0f, 100f - GetCurrentFiveStarRate() - GetCurrentFourStarRate());

        public bool TryPullSingle(out GachaPullResult result)
        {
            result = default;
            if (!TryGetActiveBanner(out var banner)) return false;
            if (currencyManager == null || inventoryManager == null) return false;
            if (!currencyManager.TrySpendCurrency(CurrencyType.Gems, banner.costPerPull)) return false;

            result = gachaSystem.PerformPull(Pity.State, bannerController.CharacterPool, banner.featuredCharacterId, GetOwnedCharacterIds());
            inventoryManager.ApplyPullResult(result);
            shardManager?.HandlePullResult(result);
            History.Record(result);
            return true;
        }

        public bool TryPullTen(out List<GachaPullResult> results)
        {
            results = null;
            if (!TryGetActiveBanner(out var banner)) return false;
            if (currencyManager == null || inventoryManager == null) return false;

            int totalCost = banner.costPerPull * TenPullCount;
            if (!currencyManager.TrySpendCurrency(CurrencyType.Gems, totalCost)) return false;

            results = gachaSystem.PerformTenPull(Pity.State, bannerController.CharacterPool, banner.featuredCharacterId, GetOwnedCharacterIds());
            foreach (var result in results)
            {
                inventoryManager.ApplyPullResult(result);
                shardManager?.HandlePullResult(result);
                History.Record(result);
            }
            return true;
        }

        private bool TryGetActiveBanner(out BannerData banner)
        {
            banner = null;
            if (bannerController == null) return false;

            banner = bannerController.ActiveBanner;
            return banner != null;
        }

        private HashSet<string> GetOwnedCharacterIds()
        {
            var ids = new HashSet<string>();
            if (inventoryManager == null) return ids;

            foreach (var kvp in inventoryManager.OwnedCharacters) ids.Add(kvp.Value.baseDataId);
            return ids;
        }
    }
}
