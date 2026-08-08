using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Managers;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Network
{
    [Serializable]
    public class SkinSelectionEntry
    {
        public string characterInstanceId;
        public string skinId;
    }

    [Serializable]
    public class SkinSyncRequest
    {
        public List<SkinSelectionEntry> selections;
    }

    // Pushes each character's equipped skin choice to the backend profile, so cosmetic selections
    // persist across devices/reinstalls the same way ExpeditionSyncService does for expedition timers.
    public class SkinSyncService : MonoBehaviour, IService
    {
        private NetworkManager networkManager;
        private InventoryManager inventoryManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            ServiceLocator.Instance.TryGet(out inventoryManager);
            Debug.Log($"{nameof(SkinSyncService)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task<bool> SyncEquippedSkinsAsync()
        {
            if (networkManager == null || inventoryManager == null) return false;

            var request = new SkinSyncRequest
            {
                selections = inventoryManager.OwnedCharacters.Values
                    .Where(c => !string.IsNullOrEmpty(c.equippedSkinId))
                    .Select(c => new SkinSelectionEntry { characterInstanceId = c.instanceId, skinId = c.equippedSkinId })
                    .ToList()
            };

            var response = await networkManager.PostAsync<object>("/skins/sync", request);
            return response.success;
        }
    }
}
