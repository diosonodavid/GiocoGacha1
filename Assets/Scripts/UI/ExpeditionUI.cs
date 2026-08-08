using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Lets the player pick an expedition and assign eligible party members before dispatching them;
    // the per-slot timer/claim widget itself is handled by ExpeditionSlotView.
    public class ExpeditionUI : UIController
    {
        [SerializeField] private Transform expeditionListContainer;
        [SerializeField] private GameObject expeditionEntryPrefab;

        private ExpeditionManager expeditionManager;
        private ExpeditionData selectedExpedition;
        private readonly List<string> selectedCharacterInstanceIds = new();

        public ExpeditionData SelectedExpedition => selectedExpedition;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out expeditionManager);
        }

        public void BuildExpeditionList(IEnumerable<ExpeditionData> availableExpeditions)
        {
            if (expeditionListContainer == null || expeditionEntryPrefab == null) return;

            for (int i = expeditionListContainer.childCount - 1; i >= 0; i--)
                Destroy(expeditionListContainer.GetChild(i).gameObject);

            foreach (var expedition in availableExpeditions)
            {
                if (expedition == null) continue;

                var go = Instantiate(expeditionEntryPrefab, expeditionListContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{expedition.expeditionName} ({expedition.durationMinutes}m)";

                var button = go.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => SelectExpedition(expedition));
            }
        }

        public void SelectExpedition(ExpeditionData expedition)
        {
            selectedExpedition = expedition;
            selectedCharacterInstanceIds.Clear();
        }

        public void ToggleCharacterAssignment(CharacterInstance character)
        {
            if (character == null || selectedExpedition == null) return;

            if (selectedCharacterInstanceIds.Contains(character.instanceId))
            {
                selectedCharacterInstanceIds.Remove(character.instanceId);
                return;
            }

            if (selectedCharacterInstanceIds.Count >= selectedExpedition.teamCapacity) return;
            selectedCharacterInstanceIds.Add(character.instanceId);
        }

        public bool TryDispatch(string slotId)
        {
            if (expeditionManager == null || selectedExpedition == null) return false;
            return expeditionManager.StartExpedition(slotId, selectedExpedition, new List<string>(selectedCharacterInstanceIds));
        }
    }
}
