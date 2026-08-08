using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class StoryDialogueUI : UIController
    {
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Image speakerPortraitImage;
        [SerializeField] private Text dialogueText;
        [SerializeField] private Button skipButton;
        [SerializeField] private DialogueChoiceUI choiceUI;

        private DialogueManager dialogueManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out dialogueManager);
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueLineChanged += HandleLineChanged;
                dialogueManager.OnDialogueEnded += Hide;
            }

            if (skipButton != null) skipButton.onClick.AddListener(HandleSkipPressed);
            if (dialogueManager?.CurrentLine != null) HandleLineChanged(dialogueManager.CurrentLine);
        }

        protected override void OnHidden()
        {
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueLineChanged -= HandleLineChanged;
                dialogueManager.OnDialogueEnded -= Hide;
            }

            if (skipButton != null) skipButton.onClick.RemoveListener(HandleSkipPressed);
        }

        private void HandleLineChanged(DialogueData line)
        {
            if (line == null) return;

            if (speakerNameText != null) speakerNameText.text = line.speakerName;
            if (dialogueText != null) dialogueText.text = line.text;
            if (speakerPortraitImage != null) speakerPortraitImage.sprite = line.speakerPortrait;

            if (choiceUI != null)
            {
                if (line.choices.Count > 0) choiceUI.ShowChoices(line.choices, dialogueManager.Advance);
                else choiceUI.HideChoices();
            }
        }

        private void HandleSkipPressed() => dialogueManager?.EndDialogue();
    }
}
