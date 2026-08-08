using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    // Tracks unlocked/equipped honorific titles and profile emblems as plain id strings; the
    // actual TitleData/EmblemData catalogs (display text, icon) are left to UI/ScriptableObject
    // lookups elsewhere, the same "gather ids, let the UI resolve display data" split used by
    // AchievementManager's progress tracking versus AchievementData's display fields.
    public class TitleHonorManager : MonoBehaviour, IService
    {
        public event Action<string> OnEquippedTitleChanged;
        public event Action<string> OnEquippedEmblemChanged;

        private readonly HashSet<string> unlockedTitleIds = new();
        private readonly HashSet<string> unlockedEmblemIds = new();

        public string EquippedTitleId { get; private set; }
        public string EquippedEmblemId { get; private set; }
        public IReadOnlyCollection<string> UnlockedTitleIds => unlockedTitleIds;
        public IReadOnlyCollection<string> UnlockedEmblemIds => unlockedEmblemIds;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(TitleHonorManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void UnlockTitle(string titleId)
        {
            if (!string.IsNullOrEmpty(titleId)) unlockedTitleIds.Add(titleId);
        }

        public void UnlockEmblem(string emblemId)
        {
            if (!string.IsNullOrEmpty(emblemId)) unlockedEmblemIds.Add(emblemId);
        }

        public bool TryEquipTitle(string titleId)
        {
            if (!unlockedTitleIds.Contains(titleId)) return false;

            EquippedTitleId = titleId;
            OnEquippedTitleChanged?.Invoke(titleId);
            return true;
        }

        public bool TryEquipEmblem(string emblemId)
        {
            if (!unlockedEmblemIds.Contains(emblemId)) return false;

            EquippedEmblemId = emblemId;
            OnEquippedEmblemChanged?.Invoke(emblemId);
            return true;
        }
    }
}
