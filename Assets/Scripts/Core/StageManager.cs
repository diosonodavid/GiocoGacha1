using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Core
{
    // Tracks campaign progression: which stages are unlocked and the best star rating earned on
    // each, and auto-unlocks the next stage in sequence (within a chapter, then into the next
    // chapter) the first time a stage is cleared.
    public class StageManager : MonoBehaviour, IService
    {
        public event Action<string> OnStageUnlocked;
        public event Action<string, int> OnStageStarsUpdated;

        [SerializeField] private List<ChapterData> chapters = new();

        private readonly HashSet<string> unlockedStageIds = new();
        private readonly Dictionary<string, int> starsByStageId = new();

        public IReadOnlyList<ChapterData> Chapters => chapters;
        public IReadOnlyCollection<string> UnlockedStageIds => unlockedStageIds;
        public IReadOnlyDictionary<string, int> StarsByStageId => starsByStageId;

        public Task InitializeAsync()
        {
            if (chapters.Count > 0 && chapters[0].stages.Count > 0)
                UnlockStage(chapters[0].stages[0].stageId);

            Debug.Log($"{nameof(StageManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool IsStageUnlocked(string stageId) => !string.IsNullOrEmpty(stageId) && unlockedStageIds.Contains(stageId);

        public int GetStars(string stageId) => stageId != null && starsByStageId.TryGetValue(stageId, out var stars) ? stars : 0;

        public void UnlockStage(string stageId)
        {
            if (string.IsNullOrEmpty(stageId) || !unlockedStageIds.Add(stageId)) return;
            OnStageUnlocked?.Invoke(stageId);
        }

        // Records the best star result for a stage (never downgrades a previously higher score)
        // and unlocks the next stage in the campaign sequence.
        public void ReportStageCleared(string stageId, int starsEarned)
        {
            if (string.IsNullOrEmpty(stageId)) return;

            starsEarned = Mathf.Clamp(starsEarned, 0, GameConstants.MaxStarsPerStage);
            if (starsEarned > GetStars(stageId))
            {
                starsByStageId[stageId] = starsEarned;
                OnStageStarsUpdated?.Invoke(stageId, starsEarned);
            }

            UnlockStage(stageId);
            UnlockNextStage(stageId);
        }

        // Hydrates progression from previously saved data; additive only, so it can run right
        // after a save load without clearing anything unlocked earlier this session.
        public void LoadState(IEnumerable<string> savedUnlockedStageIds, IEnumerable<KeyValuePair<string, int>> savedStars)
        {
            if (savedUnlockedStageIds != null)
                foreach (var stageId in savedUnlockedStageIds) unlockedStageIds.Add(stageId);

            if (savedStars != null)
                foreach (var entry in savedStars) starsByStageId[entry.Key] = entry.Value;
        }

        private void UnlockNextStage(string clearedStageId)
        {
            for (int chapterIndex = 0; chapterIndex < chapters.Count; chapterIndex++)
            {
                var stages = chapters[chapterIndex].stages;
                int stageIndex = stages.FindIndex(s => s.stageId == clearedStageId);
                if (stageIndex < 0) continue;

                if (stageIndex + 1 < stages.Count)
                {
                    UnlockStage(stages[stageIndex + 1].stageId);
                    return;
                }

                if (chapterIndex + 1 < chapters.Count && chapters[chapterIndex + 1].stages.Count > 0)
                    UnlockStage(chapters[chapterIndex + 1].stages[0].stageId);
                return;
            }
        }
    }
}
