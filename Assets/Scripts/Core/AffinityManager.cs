using System;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Core
{
    // Tracks each character's friendship/affinity progress from gifting, and gates
    // CharacterStoryData chapters behind the reached level. Voice-line and stat-bonus unlocks are
    // left as future integration points reading CharacterInstance.affinityLevel directly, since
    // VoiceLineData and combat stat application carry no affinity-gating fields yet.
    public class AffinityManager : MonoBehaviour, IService
    {
        public const int PointsPerLevel = 100;

        [SerializeField] private int maxAffinityLevel = 10;

        public event Action<CharacterInstance, int> OnAffinityLevelChanged;

        private InventoryManager inventoryManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            Debug.Log($"{nameof(AffinityManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool GiveGift(CharacterInstance instance, GiftItemData gift)
        {
            if (instance == null || gift == null || inventoryManager == null) return false;
            if (!inventoryManager.TryConsumeMaterial(gift.itemId, 1)) return false;

            AddAffinityPoints(instance, gift.GetAffinityPoints(instance.baseDataId));
            return true;
        }

        public void AddAffinityPoints(CharacterInstance instance, int amount)
        {
            if (instance == null || amount <= 0) return;

            instance.affinityPoints += amount;
            int newLevel = Mathf.Min(maxAffinityLevel, instance.affinityPoints / PointsPerLevel);
            if (newLevel == instance.affinityLevel) return;

            instance.affinityLevel = newLevel;
            OnAffinityLevelChanged?.Invoke(instance, newLevel);
        }

        public bool TryUnlockStory(CharacterInstance instance, CharacterStoryData story, StoryChapter chapter)
        {
            if (instance == null || story == null || chapter == null) return false;
            if (instance.affinityLevel < chapter.requiredAffinityLevel) return false;
            if (!instance.unlockedStoryIds.Contains(chapter.chapterId))
                instance.unlockedStoryIds.Add(chapter.chapterId);
            return true;
        }
    }
}
