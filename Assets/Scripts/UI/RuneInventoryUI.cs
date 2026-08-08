using System;
using System.Collections.Generic;
using System.Linq;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Inventory;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Rune list with the same stat-based filtering as InventoryUI's gear list (reuses
    // GearStatFilter), plus equip and enhance actions against a selected character/rune.
    public class RuneInventoryUI : UIController
    {
        [SerializeField] private Transform runeListContainer;
        [SerializeField] private GameObject runeEntryPrefab;
        [SerializeField] private Text selectedRuneDetailsText;

        private InventoryManager inventoryManager;
        private RuneEnhancer runeEnhancer;
        private GearStatFilter activeFilter = GearStatFilter.All;
        private RuneData selectedRune;

        public RuneData SelectedRune => selectedRune;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            ServiceLocator.Instance.TryGet(out runeEnhancer);
            RefreshRuneList();
        }

        public void SetFilter(GearStatFilter filter)
        {
            activeFilter = filter;
            RefreshRuneList();
        }

        public void RefreshRuneList()
        {
            if (inventoryManager == null || runeListContainer == null || runeEntryPrefab == null) return;

            for (int i = runeListContainer.childCount - 1; i >= 0; i--)
                Destroy(runeListContainer.GetChild(i).gameObject);

            foreach (var rune in GetFilteredRunes())
            {
                var go = Instantiate(runeEntryPrefab, runeListContainer);
                var button = go.GetComponent<Button>();
                if (button != null) button.onClick.AddListener(() => SelectRune(rune));
            }
        }

        private IEnumerable<RuneData> GetFilteredRunes()
        {
            if (activeFilter == GearStatFilter.All) return inventoryManager.OwnedRunes;

            var targetStat = (StatType)Enum.Parse(typeof(StatType), activeFilter.ToString());
            return inventoryManager.OwnedRunes.Where(rune =>
                rune.mainStat.statType == targetStat || rune.subStats.Any(s => s.statType == targetStat));
        }

        public void SelectRune(RuneData rune)
        {
            selectedRune = rune;
            RefreshSelectedRuneDetails();
        }

        public bool TryEnhanceSelectedRune()
        {
            if (selectedRune == null || runeEnhancer == null) return false;
            bool enhanced = runeEnhancer.TryEnhance(selectedRune);
            if (enhanced) RefreshSelectedRuneDetails();
            return enhanced;
        }

        public bool TryEquipSelectedRune(CharacterInstance target)
        {
            if (selectedRune == null || target == null) return false;
            target.equippedRunes.Dictionary[selectedRune.slotIndex.ToString()] = selectedRune;
            return true;
        }

        private void RefreshSelectedRuneDetails()
        {
            if (selectedRuneDetailsText == null) return;
            if (selectedRune == null)
            {
                selectedRuneDetailsText.text = string.Empty;
                return;
            }

            selectedRuneDetailsText.text =
                $"Slot {selectedRune.slotIndex} | {selectedRune.rarity} | +{selectedRune.refinementLevel}\n" +
                $"{selectedRune.mainStat.statType}: {selectedRune.mainStat.value}{(selectedRune.mainStat.isPercentage ? "%" : "")}";
        }
    }
}
