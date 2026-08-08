using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    // Drives a single active tutorial sequence step-by-step; TutorialMaskUI and TutorialDialogueUI
    // both listen for OnStepStarted to render the highlight/dialogue for the current step.
    // Completed step ids persist to PlayerPrefs (a lightweight flag store, separate from the
    // encrypted PlayerSaveData pipeline in SaveSystem) so a sequence never replays after completion.
    public class TutorialManager : MonoBehaviour, IService
    {
        private const string CompletedStepPrefsKey = "Tutorial_CompletedSteps";

        public event Action<TutorialStepData> OnStepStarted;
        public event Action<TutorialStepData> OnStepCompleted;
        public event Action OnTutorialCompleted;

        private readonly HashSet<string> completedStepIds = new();
        private List<TutorialStepData> activeSequence;
        private int activeStepIndex = -1;

        public bool IsTutorialActive => activeSequence != null && activeStepIndex >= 0 && activeStepIndex < activeSequence.Count;
        public TutorialStepData CurrentStep => IsTutorialActive ? activeSequence[activeStepIndex] : null;

        public Task InitializeAsync()
        {
            LoadCompletedSteps();
            Debug.Log($"{nameof(TutorialManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool IsStepCompleted(string stepId) => stepId != null && completedStepIds.Contains(stepId);

        public void StartSequence(List<TutorialStepData> sequence)
        {
            if (sequence == null || sequence.Count == 0) return;

            activeSequence = sequence;
            activeStepIndex = 0;
            OnStepStarted?.Invoke(CurrentStep);
        }

        public void CompleteCurrentStep()
        {
            if (!IsTutorialActive) return;

            var step = CurrentStep;
            completedStepIds.Add(step.stepId);
            SaveCompletedSteps();
            OnStepCompleted?.Invoke(step);

            activeStepIndex++;
            if (IsTutorialActive)
                OnStepStarted?.Invoke(CurrentStep);
            else
                EndSequence();
        }

        private void EndSequence()
        {
            activeSequence = null;
            activeStepIndex = -1;
            OnTutorialCompleted?.Invoke();
        }

        private void LoadCompletedSteps()
        {
            completedStepIds.Clear();
            string stored = PlayerPrefs.GetString(CompletedStepPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(stored)) return;

            foreach (var id in stored.Split(','))
                if (!string.IsNullOrEmpty(id)) completedStepIds.Add(id);
        }

        private void SaveCompletedSteps()
        {
            PlayerPrefs.SetString(CompletedStepPrefsKey, string.Join(",", completedStepIds));
            PlayerPrefs.Save();
        }
    }
}
