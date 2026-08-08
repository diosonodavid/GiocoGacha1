using System.Collections;
using GachaGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Reveals the current tutorial step's dialogue text one character at a time; pressing continue
    // either fast-forwards the reveal or, once fully revealed, advances to the next step.
    public class TutorialDialogueUI : UIController
    {
        [SerializeField] private Text dialogueText;
        [SerializeField] private Button continueButton;
        [SerializeField] private float charactersPerSecond = 40f;

        private TutorialManager tutorialManager;
        private Coroutine typingRoutine;
        private string fullText = string.Empty;
        private bool isTyping;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out tutorialManager);
            if (tutorialManager != null) tutorialManager.OnStepStarted += HandleStepStarted;
            if (continueButton != null) continueButton.onClick.AddListener(HandleContinuePressed);

            if (tutorialManager?.CurrentStep != null) HandleStepStarted(tutorialManager.CurrentStep);
        }

        protected override void OnHidden()
        {
            if (tutorialManager != null) tutorialManager.OnStepStarted -= HandleStepStarted;
            if (continueButton != null) continueButton.onClick.RemoveListener(HandleContinuePressed);
            if (typingRoutine != null) StopCoroutine(typingRoutine);
        }

        private void HandleStepStarted(TutorialStepData step)
        {
            if (step == null) return;
            fullText = step.dialogueText ?? string.Empty;

            if (typingRoutine != null) StopCoroutine(typingRoutine);
            typingRoutine = StartCoroutine(TypeText());
        }

        private IEnumerator TypeText()
        {
            isTyping = true;
            if (dialogueText != null) dialogueText.text = string.Empty;

            float secondsPerChar = charactersPerSecond > 0f ? 1f / charactersPerSecond : 0f;
            for (int i = 0; i < fullText.Length; i++)
            {
                if (dialogueText != null) dialogueText.text = fullText.Substring(0, i + 1);
                if (secondsPerChar > 0f) yield return new WaitForSeconds(secondsPerChar);
            }

            isTyping = false;
            typingRoutine = null;
        }

        private void HandleContinuePressed()
        {
            if (isTyping)
            {
                SkipTyping();
                return;
            }

            if (tutorialManager != null && tutorialManager.CurrentStep != null && !tutorialManager.CurrentStep.actionRequired)
                tutorialManager.CompleteCurrentStep();
        }

        public void SkipTyping()
        {
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            if (dialogueText != null) dialogueText.text = fullText;
            isTyping = false;
            typingRoutine = null;
        }
    }
}
