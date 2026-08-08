using System;
using System.Collections.Generic;
using System.Linq;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public enum GearStatFilter { All, HP, ATK, DEF, SPD, CritRate, CritDMG }

    // Gear list with stat-based filtering, an equip flow against a selected character, and an
    // upgrade action that increments GearData.enhanceLevel after CurrencyManager confirms the cost.
    public class InventoryUI : MonoBehaviour
    {
        private const int MaxEnhanceLevel = 15;
        private const int EnhanceCostPerLevel = 500;

        [SerializeField] private Transform gearListContainer;
        [SerializeField] private GameObject gearEntryPrefab;
        [SerializeField] private Text selectedGearDetailsText;

        private InventoryManager inventoryManager;
        private CurrencyManager currencyManager;
        private GearStatFilter activeFilter = GearStatFilter.All;
        private GearData selectedGear;

        public GearData SelectedGear => selectedGear;

        private void OnEnable()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            ServiceLocator.Instance.TryGet(out currencyManager);
            RefreshGearList();
        }

        public void SetFilter(GearStatFilter filter)
        {
            activeFilter = filter;
            RefreshGearList();
        }

        public void RefreshGearList()
        {
            if (inventoryManager == null || gearListContainer == null || gearEntryPrefab == null) return;

            for (int i = gearListContainer.childCount - 1; i >= 0; i--)
                Destroy(gearListContainer.GetChild(i).gameObject);

            foreach (var gear in GetFilteredGear())
            {
                var go = Instantiate(gearEntryPrefab, gearListContainer);
                var button = go.GetComponent<Button>();
                if (button != null) button.onClick.AddListener(() => SelectGear(gear));
            }
        }

        private IEnumerable<GearData> GetFilteredGear()
        {
            if (activeFilter == GearStatFilter.All) return inventoryManager.OwnedGear;

            var targetStat = (StatType)Enum.Parse(typeof(StatType), activeFilter.ToString());
            return inventoryManager.OwnedGear.Where(gear =>
                gear.mainStat.statType == targetStat || gear.subStats.Any(s => s.statType == targetStat));
        }

        public void SelectGear(GearData gear)
        {
            selectedGear = gear;
            RefreshSelectedGearDetails();
        }

        public bool TryUpgradeSelectedGear()
        {
            if (selectedGear == null || currencyManager == null) return false;
            if (selectedGear.enhanceLevel >= MaxEnhanceLevel) return false;

            int cost = EnhanceCostPerLevel * (selectedGear.enhanceLevel + 1);
            if (!currencyManager.TrySpendCurrency(CurrencyType.Gold, cost)) return false;

            selectedGear.enhanceLevel++;
            RefreshSelectedGearDetails();
            return true;
        }

        public bool TryEquipSelectedGear(CharacterInstance target)
        {
            if (selectedGear == null || target == null) return false;
            target.equippedGear.Dictionary[selectedGear.slotType.ToString()] = selectedGear;
            return true;
        }

        private void RefreshSelectedGearDetails()
        {
            if (selectedGearDetailsText == null) return;
            if (selectedGear == null)
            {
                selectedGearDetailsText.text = string.Empty;
                return;
            }

            selectedGearDetailsText.text =
                $"{selectedGear.slotType} | {selectedGear.rarity} | +{selectedGear.enhanceLevel}\n" +
                $"{selectedGear.mainStat.statType}: {selectedGear.mainStat.value}{(selectedGear.mainStat.isPercentage ? "%" : "")}";
        }
    }
}
