using System.Collections.Generic;
using GachaGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class DormitoryUI : UIController
    {
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotViewPrefab;
        [SerializeField] private Text affectionText;
        [SerializeField] private List<DormitoryCharacterSlot> slots = new();

        private DormitoryCharacterSlot selectedSlot;

        protected override void OnShown() => RebuildSlots();

        public void AssignCharacterToSelectedSlot(string characterInstanceId)
        {
            selectedSlot?.AssignCharacter(characterInstanceId);
            RefreshSelectedSlot();
        }

        private void RebuildSlots()
        {
            if (slotContainer == null) return;

            for (int i = slotContainer.childCount - 1; i >= 0; i--)
                Destroy(slotContainer.GetChild(i).gameObject);

            foreach (var slot in slots)
            {
                if (slot == null || slotViewPrefab == null) continue;

                var view = Instantiate(slotViewPrefab, slotContainer);
                var button = view.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => SelectSlot(slot));
            }
        }

        private void SelectSlot(DormitoryCharacterSlot slot)
        {
            selectedSlot = slot;
            RefreshSelectedSlot();
        }

        private void RefreshSelectedSlot()
        {
            if (affectionText != null)
                affectionText.text = selectedSlot != null ? selectedSlot.affectionLevel.ToString() : string.Empty;
        }
    }
}
