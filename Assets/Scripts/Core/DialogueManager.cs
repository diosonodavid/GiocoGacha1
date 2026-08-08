using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Core
{
    // Walks a chain of DialogueData assets by id (nextDialogueId, or a branch chosen from
    // choices), playing each line's voice clip through AudioManager's existing voice channel.
    public class DialogueManager : MonoBehaviour, IService
    {
        public event Action<DialogueData> OnDialogueLineChanged;
        public event Action OnDialogueEnded;

        [SerializeField] private List<DialogueData> dialogueCatalog = new();

        private Dictionary<string, DialogueData> dialogueLookup;
        private AudioManager audioManager;
        private DialogueData currentLine;

        public DialogueData CurrentLine => currentLine;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out audioManager);
            dialogueLookup = dialogueCatalog.Where(d => d != null).ToDictionary(d => d.dialogueId, d => d);
            Debug.Log($"{nameof(DialogueManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void StartDialogue(string dialogueId) => PlayLine(dialogueId);

        // Pass a choice's nextDialogueId when the current line has branches, otherwise pass null to
        // follow the line's own nextDialogueId.
        public void Advance(string chosenNextDialogueId = null)
        {
            string nextId = chosenNextDialogueId ?? currentLine?.nextDialogueId;
            if (string.IsNullOrEmpty(nextId))
            {
                EndDialogue();
                return;
            }

            PlayLine(nextId);
        }

        public void EndDialogue()
        {
            currentLine = null;
            OnDialogueEnded?.Invoke();
        }

        private void PlayLine(string dialogueId)
        {
            if (dialogueLookup == null || !dialogueLookup.TryGetValue(dialogueId, out var line))
            {
                EndDialogue();
                return;
            }

            currentLine = line;
            if (line.audioClip != null) audioManager?.PlayVoice(line.audioClip);
            OnDialogueLineChanged?.Invoke(line);
        }
    }
}
