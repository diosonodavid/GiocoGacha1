using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class HousingUI : UIController
    {
        [SerializeField] private Transform roomContainer;
        [SerializeField] private Transform inventoryListContainer;
        [SerializeField] private GameObject furnitureIconPrefab;
        [SerializeField] private Text comfortText;
        [SerializeField] private List<FurnitureData> furnitureInventory = new();

        private HousingManager housingManager;
        private FurnitureData selectedFurniture;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out housingManager);
            RebuildInventory();
            RebuildRoom();
        }

        public void HandlePlaceInRoom(Vector2 position)
        {
            if (selectedFurniture == null || housingManager == null) return;
            housingManager.PlaceFurniture(selectedFurniture, position);
            RebuildRoom();
        }

        private void RebuildInventory()
        {
            if (inventoryListContainer == null) return;
            ClearChildren(inventoryListContainer);

            foreach (var furniture in furnitureInventory)
            {
                if (furniture == null || furnitureIconPrefab == null) continue;

                var entry = Instantiate(furnitureIconPrefab, inventoryListContainer);
                var icon = entry.GetComponentInChildren<Image>();
                if (icon != null) icon.sprite = furniture.icon;

                var button = entry.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => selectedFurniture = furniture);
            }
        }

        private void RebuildRoom()
        {
            if (roomContainer == null || housingManager == null) return;
            ClearChildren(roomContainer);

            foreach (var placed in housingManager.PlacedFurniture)
            {
                if (furnitureIconPrefab == null) continue;

                var view = Instantiate(furnitureIconPrefab, roomContainer);
                var rect = view.GetComponent<RectTransform>();
                if (rect != null) rect.anchoredPosition = placed.roomPosition;

                var icon = view.GetComponentInChildren<Image>();
                if (icon != null && placed.data != null) icon.sprite = placed.data.icon;
            }

            if (comfortText != null) comfortText.text = housingManager.TotalComfort.ToString();
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }
    }
}
