using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Inventory
{
    // Tracks which skins the player has unlocked and equips one at a time per CharacterInstance.
    // Equipping only records the choice (CharacterInstance.equippedSkinId) - resolving that id into
    // an actual battle model is SkinApplier's job, kept separate so this stays testable without a scene.
    public class SkinManager : MonoBehaviour, IService
    {
        private readonly HashSet<string> unlockedSkinIds = new();

        public event Action<CharacterInstance, string> OnSkinEquipped;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(SkinManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool IsUnlocked(string skinId) => !string.IsNullOrEmpty(skinId) && unlockedSkinIds.Contains(skinId);

        public void UnlockSkin(string skinId)
        {
            if (!string.IsNullOrEmpty(skinId)) unlockedSkinIds.Add(skinId);
        }

        public bool TryEquipSkin(CharacterInstance instance, SkinData skin)
        {
            if (instance == null || skin == null) return false;
            if (skin.characterId != instance.baseDataId) return false;
            if (!IsUnlocked(skin.skinId)) return false;

            instance.equippedSkinId = skin.skinId;
            OnSkinEquipped?.Invoke(instance, skin.skinId);
            return true;
        }
    }
}
