using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Managers
{
    public class InventoryManager : MonoBehaviour, IService
    {
        private readonly Dictionary<string, CharacterInstance> ownedCharacters = new();
        private readonly Dictionary<string, int> materials = new();
        private readonly Dictionary<string, int> fragments = new();
        private readonly List<GearData> ownedGear = new();
        private readonly List<RuneData> ownedRunes = new();

        public IReadOnlyDictionary<string, CharacterInstance> OwnedCharacters => ownedCharacters;
        public IReadOnlyList<GearData> OwnedGear => ownedGear;
        public IReadOnlyList<RuneData> OwnedRunes => ownedRunes;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(InventoryManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool HasCharacter(string baseDataId) => baseDataId != null && ownedCharacters.ContainsKey(baseDataId);

        // Adds a newly pulled character, or converts the duplicate into a memory level (capped at
        // GameConstants.MaxMemory) - once maxed, further duplicates become memory fragments instead.
        public void ApplyPullResult(GachaPullResult result)
        {
            if (result.pulledCharacter == null) return;
            string baseDataId = result.pulledCharacter.id;

            if (!ownedCharacters.TryGetValue(baseDataId, out var instance))
            {
                ownedCharacters[baseDataId] = new CharacterInstance
                {
                    instanceId = Guid.NewGuid().ToString(),
                    baseDataId = baseDataId,
                    currentLevel = 1,
                    currentExp = 0,
                    memoryLevel = 0
                };
                return;
            }

            if (instance.memoryLevel < GameConstants.MaxMemory)
                instance.memoryLevel++;
            else
                AddFragments(GetMemoryFragmentId(result.rarity), 1);
        }

        public static string GetMemoryFragmentId(Rarity rarity) => $"memory_fragment_{rarity}";

        public void AddMaterial(string itemId, int amount)
        {
            if (itemId == null || amount <= 0) return;
            materials[itemId] = GetMaterialCount(itemId) + amount;
        }

        public bool TryConsumeMaterial(string itemId, int amount)
        {
            if (itemId == null || amount <= 0) return true;
            if (GetMaterialCount(itemId) < amount) return false;
            materials[itemId] -= amount;
            return true;
        }

        public int GetMaterialCount(string itemId) => itemId != null && materials.TryGetValue(itemId, out var count) ? count : 0;

        public void AddFragments(string fragmentId, int amount)
        {
            if (fragmentId == null || amount <= 0) return;
            fragments[fragmentId] = GetFragmentCount(fragmentId) + amount;
        }

        public int GetFragmentCount(string fragmentId) => fragmentId != null && fragments.TryGetValue(fragmentId, out var count) ? count : 0;

        public void AddGear(GearData gear)
        {
            if (gear != null) ownedGear.Add(gear);
        }

        public void RemoveGear(GearData gear) => ownedGear.Remove(gear);

        public void AddRune(RuneData rune)
        {
            if (rune != null) ownedRunes.Add(rune);
        }

        public void RemoveRune(RuneData rune) => ownedRunes.Remove(rune);
    }
}
