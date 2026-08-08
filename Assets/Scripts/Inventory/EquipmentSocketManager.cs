using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Inventory
{
    // Moves gems already owned via InventoryManager.OwnedGems into/out of a specific gear piece's
    // sockets, keyed by GearData reference (plain class, no Equals/GetHashCode override, so this is
    // per-instance like BreakSystem's Dictionary<ICombatant, ...>). Stat totals here are compute-only,
    // like EquipmentSetManager.GetActiveSetBonuses - applying them into combat stats is left as a
    // future StatCalculator integration point.
    public class EquipmentSocketManager : MonoBehaviour, IService
    {
        public const int MaxSocketsPerGear = 4;

        public event Action<GearData, GemData> OnGemSocketed;
        public event Action<GearData, GemData> OnGemRemoved;

        private readonly Dictionary<GearData, List<GemData>> socketedGemsByGear = new();
        private InventoryManager inventoryManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            Debug.Log($"{nameof(EquipmentSocketManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public IReadOnlyList<GemData> GetSocketedGems(GearData gear) =>
            gear != null && socketedGemsByGear.TryGetValue(gear, out var list) ? list : Array.Empty<GemData>();

        public bool TryInsertGem(GearData gear, GemData gem)
        {
            if (gear == null || gem == null || inventoryManager == null) return false;
            if (!inventoryManager.OwnedGems.Contains(gem)) return false;

            if (!socketedGemsByGear.TryGetValue(gear, out var list))
                socketedGemsByGear[gear] = list = new List<GemData>();

            if (list.Count >= MaxSocketsPerGear) return false;

            inventoryManager.RemoveGem(gem);
            list.Add(gem);
            OnGemSocketed?.Invoke(gear, gem);
            return true;
        }

        public bool RemoveGem(GearData gear, GemData gem)
        {
            if (gear == null || gem == null || inventoryManager == null) return false;
            if (!socketedGemsByGear.TryGetValue(gear, out var list) || !list.Remove(gem)) return false;

            inventoryManager.AddGem(gem);
            OnGemRemoved?.Invoke(gear, gem);
            return true;
        }

        public Dictionary<StatType, float> GetTotalStatBonus(GearData gear)
        {
            var result = new Dictionary<StatType, float>();

            foreach (var gem in GetSocketedGems(gear))
            {
                if (gem?.statBonus == null) continue;
                result[gem.statBonus.statType] = (result.TryGetValue(gem.statBonus.statType, out var existing) ? existing : 0f) + gem.statBonus.value;
            }

            return result;
        }
    }
}
