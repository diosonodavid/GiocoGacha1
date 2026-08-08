using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Inventory;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Gear enhancement screen: pick an owned piece of gear, see its next-level material/gold
    // cost (EquipmentEnhancer), and enhance it.
    public class EnhancementUI : UIController
    {
        [SerializeField] private Transform gearListContainer;
        [SerializeField] private GameObject gearEntryPrefab;
        [SerializeField] private Text selectedGearText;
        [SerializeField] private Text costText;
        [SerializeField] private Button enhanceButton;

        private InventoryManager inventoryManager;
        private CurrencyManager currencyManager;
        private GearData selectedGear;

        private void Awake()
        {
            if (enhanceButton != null) enhanceButton.onClick.AddListener(EnhanceSelectedGear);
        }

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            ServiceLocator.Instance.TryGet(out currencyManager);
            RefreshGearList();
        }

        public void RefreshGearList()
        {
            if (inventoryManager == null || gearListContainer == null || gearEntryPrefab == null) return;

            ClearContainer(gearListContainer);

            foreach (var gear in inventoryManager.OwnedGear)
            {
                var go = Instantiate(gearEntryPrefab, gearListContainer);
                var button = go.GetComponent<Button>();
                if (button != null) button.onClick.AddListener(() => SelectGear(gear));
            }
        }

        public void SelectGear(GearData gear)
        {
            selectedGear = gear;
            RefreshSelectedGearDetails();
        }

        private void RefreshSelectedGearDetails()
        {
            if (selectedGear == null)
            {
                if (selectedGearText != null) selectedGearText.text = string.Empty;
                if (costText != null) costText.text = string.Empty;
                if (enhanceButton != null) enhanceButton.interactable = false;
                return;
            }

            if (selectedGearText != null)
                selectedGearText.text = $"{selectedGear.slotType} | {selectedGear.rarity} | +{selectedGear.enhanceLevel}";

            if (costText != null)
            {
                int expCost = EquipmentEnhancer.GetExpMaterialCost(selectedGear.enhanceLevel);
                int goldCost = EquipmentEnhancer.GetGoldCost(selectedGear.enhanceLevel);
                costText.text = $"{expCost} materials, {goldCost} gold";
            }

            if (enhanceButton != null)
                enhanceButton.interactable = selectedGear.enhanceLevel < EquipmentEnhancer.MaxEnhanceLevel;
        }

        public void EnhanceSelectedGear()
        {
            if (selectedGear == null) return;
            if (EquipmentEnhancer.TryEnhance(selectedGear, inventoryManager, currencyManager))
                RefreshSelectedGearDetails();
        }

        private static void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
