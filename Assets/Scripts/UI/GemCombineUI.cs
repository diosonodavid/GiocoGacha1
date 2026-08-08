using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Inventory;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class GemCombineUI : UIController
    {
        [SerializeField] private Transform gemListContainer;
        [SerializeField] private GameObject gemEntryPrefab;
        [SerializeField] private Text resultPreviewText;
        [SerializeField] private Button combineButton;

        private readonly List<GemData> selectedGems = new();

        private GemCombineService combineService;
        private InventoryManager inventoryManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out combineService);
            ServiceLocator.Instance.TryGet(out inventoryManager);

            if (combineButton != null) combineButton.onClick.AddListener(HandleCombinePressed);

            selectedGems.Clear();
            RebuildGemList();
            RefreshPreview();
        }

        protected override void OnHidden()
        {
            if (combineButton != null) combineButton.onClick.RemoveListener(HandleCombinePressed);
        }

        private void RebuildGemList()
        {
            if (gemListContainer == null || inventoryManager == null) return;

            for (int i = gemListContainer.childCount - 1; i >= 0; i--)
                Destroy(gemListContainer.GetChild(i).gameObject);

            foreach (var gem in inventoryManager.OwnedGems)
            {
                if (gemEntryPrefab == null) continue;

                var entry = Instantiate(gemEntryPrefab, gemListContainer);
                var label = entry.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{gem.itemName} (Lv.{gem.gemLevel})";

                var button = entry.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => ToggleSelect(gem));
            }
        }

        private void ToggleSelect(GemData gem)
        {
            if (!selectedGems.Remove(gem) && selectedGems.Count < GemCombineService.GemsRequiredToCombine)
                selectedGems.Add(gem);

            RefreshPreview();
        }

        private void RefreshPreview()
        {
            bool canCombine = combineService != null && combineService.CanCombine(selectedGems);
            if (combineButton != null) combineButton.interactable = canCombine;

            if (resultPreviewText != null)
                resultPreviewText.text = canCombine ? $"Lv.{selectedGems[0].gemLevel + 1}" : string.Empty;
        }

        private void HandleCombinePressed()
        {
            if (combineService == null) return;
            var result = combineService.TryCombine(selectedGems);
            if (result == null) return;

            selectedGems.Clear();
            RebuildGemList();
            RefreshPreview();
        }
    }
}
