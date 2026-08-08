using System;
using System.Collections.Generic;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Builds one button per DialogueData choice; invokes the callback with the chosen branch's
    // nextDialogueId so DialogueManager.Advance can be passed directly as onChoiceSelected.
    public class DialogueChoiceUI : MonoBehaviour
    {
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        public void ShowChoices(List<DialogueChoice> choices, Action<string> onChoiceSelected)
        {
            if (choiceContainer == null || choiceButtonPrefab == null) return;

            ClearChoices();
            gameObject.SetActive(true);

            foreach (var choice in choices)
            {
                if (choice == null) continue;

                var go = Instantiate(choiceButtonPrefab, choiceContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = choice.choiceText;

                var button = go.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => onChoiceSelected?.Invoke(choice.nextDialogueId));
            }
        }

        public void HideChoices()
        {
            ClearChoices();
            gameObject.SetActive(false);
        }

        private void ClearChoices()
        {
            if (choiceContainer == null) return;
            for (int i = choiceContainer.childCount - 1; i >= 0; i--)
                Destroy(choiceContainer.GetChild(i).gameObject);
        }
    }
}
