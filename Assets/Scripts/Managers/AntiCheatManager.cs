using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Managers
{
    // Client-side, defense-in-depth checks only - the server independently re-validates
    // everything it receives (see playerService.validateSyncInput on the backend), so these exist
    // to catch obviously tampered clients early and avoid wasting a round trip, not to replace
    // server authority.
    public class AntiCheatManager : MonoBehaviour, IService
    {
        private const float MaxAllowedTimeDriftRatio = 1.15f; // engine time running >15% faster than wall clock
        private const float DriftCheckIntervalSeconds = 5f;
        private const float StatTolerance = 1.001f; // float rounding slack

        public event Action OnSpeedhackDetected;
        public event Action<string> OnStatInconsistencyDetected;

        private float engineTimeAccumulator;
        private DateTime lastWallClockCheck;
        private float intervalTimer;

        public Task InitializeAsync()
        {
            lastWallClockCheck = DateTime.UtcNow;
            Debug.Log($"{nameof(AntiCheatManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        private void Update()
        {
            engineTimeAccumulator += Time.unscaledDeltaTime;
            intervalTimer += Time.unscaledDeltaTime;

            if (intervalTimer < DriftCheckIntervalSeconds) return;
            intervalTimer = 0f;

            CheckForSpeedhack();
        }

        private void CheckForSpeedhack()
        {
            var now = DateTime.UtcNow;
            double wallSeconds = (now - lastWallClockCheck).TotalSeconds;
            lastWallClockCheck = now;

            if (wallSeconds <= 0.01) return; // guard against a paused/backgrounded app skewing the ratio

            float ratio = engineTimeAccumulator / (float)wallSeconds;
            engineTimeAccumulator = 0f;

            if (ratio > MaxAllowedTimeDriftRatio)
            {
                Debug.LogWarning($"Potential speedhack detected: engine/wall-clock ratio {ratio:F2}.");
                OnSpeedhackDetected?.Invoke();
            }
        }

        // Recomputes what a character's combat stats should be from its own base stats + level +
        // gear, and flags the instance if the cached stats it is actually carrying exceed that -
        // the kind of mismatch a memory-edited client would produce, before it gets synced up.
        public bool ValidateCharacterStats(CharacterInstance instance, IReadOnlyDictionary<StatType, float> declaredBaseStats)
        {
            if (instance == null || declaredBaseStats == null) return false;

            var expected = StatCalculator.CalculateFinalStats(declaredBaseStats, instance.currentLevel, GetEquippedGear(instance));

            foreach (var kvp in expected)
            {
                if (!instance.BaseStats.TryGetValue(kvp.Key, out var actual))
                {
                    Report(instance, kvp.Key);
                    return false;
                }

                float upperBound = kvp.Value * StatTolerance;
                if (actual > upperBound)
                {
                    Report(instance, kvp.Key);
                    return false;
                }
            }

            return true;
        }

        private static IEnumerable<GearData> GetEquippedGear(CharacterInstance instance) => instance.equippedGear.Dictionary.Values;

        private void Report(CharacterInstance instance, StatType stat)
        {
            string message = $"Stat inconsistency on {instance.instanceId}: {stat} exceeds the recalculated expected value.";
            Debug.LogWarning(message);
            OnStatInconsistencyDetected?.Invoke(message);
        }
    }
}
