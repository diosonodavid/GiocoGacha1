using GachaGame.Core;
using UnityEngine;

namespace GachaGame.Utilities
{
    // Attach to a boss stage's controller; call TriggerBeforeBattle/TriggerAfterBattle from the
    // battle flow at the appropriate points to play a narrative beat via DialogueManager.
    public class CutsceneTrigger : MonoBehaviour
    {
        [SerializeField] private string dialogueIdBeforeBattle;
        [SerializeField] private string dialogueIdAfterBattle;

        private DialogueManager dialogueManager;

        private void Awake() => ServiceLocator.Instance.TryGet(out dialogueManager);

        public void TriggerBeforeBattle()
        {
            if (dialogueManager != null && !string.IsNullOrEmpty(dialogueIdBeforeBattle))
                dialogueManager.StartDialogue(dialogueIdBeforeBattle);
        }

        public void TriggerAfterBattle()
        {
            if (dialogueManager != null && !string.IsNullOrEmpty(dialogueIdAfterBattle))
                dialogueManager.StartDialogue(dialogueIdAfterBattle);
        }
    }
}
