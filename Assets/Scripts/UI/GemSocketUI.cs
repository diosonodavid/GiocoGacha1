using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Inventory;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class GemSocketUI : UIController
    {
        [SerializeField] private Transform socketSlotContainer;
        [SerializeField] private GameObject socketSlotPrefab;
        [SerializeField] private Transform gemListContainer;
        [SerializeField] private GameObject gemEntryPrefab;

        private EquipmentSocketManager socketManager;
        private InventoryManager inventoryManager;
        private GearData boundGear;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out socketManager);
            ServiceLocator.Instance.TryGet(out inventoryManager);

            if (socketManager != null)
            {
                socketManager.OnGemSocketed += HandleSocketsChanged;
                socketManager.OnGemRemoved += HandleSocketsChanged;
            }
        }

        protected override void OnHidden()
        {
            if (socketManager == null) return;
            socketManager.OnGemSocketed -= HandleSocketsChanged;
            socketManager.OnGemRemoved -= HandleSocketsChanged;
        }

        public void BindGear(GearData gear)
        {
            boundGear = gear;
            RefreshSockets();
            RefreshGemList();
        }

        private void HandleSocketsChanged(GearData gear, GemData gem)
        {
            if (gear != boundGear) return;
            RefreshSockets();
            RefreshGemList();
        }

        private void RefreshSockets()
        {
            if (socketSlotContainer == null || socketManager == null || boundGear == null) return;

            ClearContainer(socketSlotContainer);

            foreach (var gem in socketManager.GetSocketedGems(boundGear))
            {
                if (socketSlotPrefab == null) continue;

                var entry = Instantiate(socketSlotPrefab, socketSlotContainer);
                var label = entry.GetComponentInChildren<Text>();
                if (label != null) label.text = gem.itemName;

                var button = entry.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => socketManager.RemoveGem(boundGear, gem));
            }
        }

        private void RefreshGemList()
        {
            if (gemListContainer == null || inventoryManager == null || boundGear == null) return;

            ClearContainer(gemListContainer);

            foreach (var gem in inventoryManager.OwnedGems)
            {
                if (gemEntryPrefab == null) continue;

                var entry = Instantiate(gemEntryPrefab, gemListContainer);
                var label = entry.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{gem.itemName} (Lv.{gem.gemLevel})";

                var button = entry.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => socketManager.TryInsertGem(boundGear, gem));
            }
        }

        private static void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
