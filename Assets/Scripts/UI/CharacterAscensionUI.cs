using System.Linq;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Inventory;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Shows the next ascension tier's required materials (owned/required counts) and gold cost,
    // and drives the ascend button; CharacterAscensionSystem owns the actual state change, this
    // only reads it and plays the evolution animation on success.
    public class CharacterAscensionUI : UIController
    {
        [SerializeField] private Text characterNameText;
        [SerializeField] private Text currentTierText;
        [SerializeField] private Text goldCostText;
        [SerializeField] private Transform materialListContainer;
        [SerializeField] private GameObject materialEntryPrefab;
        [SerializeField] private Button ascendButton;
        [SerializeField] private Animator evolutionAnimator;
        [SerializeField] private string evolveAnimationTrigger = "Evolve";

        private CharacterAscensionSystem ascensionSystem;
        private InventoryManager inventoryManager;
        private CharacterInstance boundInstance;
        private CharacterBaseData boundBaseData;
        private AscensionData boundTable;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out ascensionSystem);
            ServiceLocator.Instance.TryGet(out inventoryManager);
        }

        public void Bind(CharacterInstance instance, CharacterBaseData baseData, AscensionData table)
        {
            boundInstance = instance;
            boundBaseData = baseData;
            boundTable = table;
            Refresh();
        }

        public void Refresh()
        {
            if (boundInstance == null || boundBaseData == null || boundTable == null || ascensionSystem == null) return;

            if (characterNameText != null) characterNameText.text = boundBaseData.characterName;
            if (currentTierText != null) currentTierText.text = $"Tier {boundInstance.ascensionLevel}";

            var nextTier = boundTable.tiers.FirstOrDefault(t => t.starLevel == boundInstance.ascensionLevel + 1);
            BuildMaterialList(nextTier);

            if (goldCostText != null) goldCostText.text = nextTier != null ? nextTier.goldCost.ToString() : "-";

            if (ascendButton != null)
            {
                ascendButton.interactable = nextTier != null && ascensionSystem.CanAscend(boundInstance, boundTable);
                ascendButton.onClick.RemoveAllListeners();
                ascendButton.onClick.AddListener(Ascend);
            }
        }

        private void BuildMaterialList(AscensionTier nextTier)
        {
            if (materialListContainer == null || materialEntryPrefab == null) return;

            for (int i = materialListContainer.childCount - 1; i >= 0; i--)
                Destroy(materialListContainer.GetChild(i).gameObject);

            if (nextTier == null) return;

            foreach (var material in nextTier.materials)
            {
                if (material.item == null) continue;

                var go = Instantiate(materialEntryPrefab, materialListContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label == null) continue;

                int owned = inventoryManager != null ? inventoryManager.GetMaterialCount(material.item.itemId) : 0;
                label.text = $"{material.item.itemName} {owned}/{material.amount}";
            }
        }

        private void Ascend()
        {
            var result = ascensionSystem.TryAscend(boundInstance, boundTable);
            if (result == AscensionResult.Success && evolutionAnimator != null)
                evolutionAnimator.SetTrigger(evolveAnimationTrigger);

            Refresh();
        }
    }
}
