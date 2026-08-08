using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Network
{
    [Serializable]
    public class ExpeditionSyncEntry
    {
        public string slotId;
        public string expeditionId;
        public long endTimeUnix;
    }

    [Serializable]
    public class ExpeditionSyncRequest
    {
        public List<ExpeditionSyncEntry> expeditions;
    }

    [Serializable]
    public class ExpeditionClaimRequest
    {
        public string slotId;
    }

    // Periodically pushes each active expedition's timer to the backend (so the countdown survives
    // a reinstall or a second device) and reports completions once the player claims a reward.
    public class ExpeditionSyncService : MonoBehaviour, IService
    {
        private NetworkManager networkManager;
        private ExpeditionManager expeditionManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            ServiceLocator.Instance.TryGet(out expeditionManager);
            Debug.Log($"{nameof(ExpeditionSyncService)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task<bool> SyncActiveExpeditionsAsync()
        {
            if (networkManager == null || expeditionManager == null) return false;

            var request = new ExpeditionSyncRequest
            {
                expeditions = expeditionManager.ActiveExpeditions.Values.Select(e => new ExpeditionSyncEntry
                {
                    slotId = e.slotId,
                    expeditionId = e.data != null ? e.data.expeditionId : null,
                    endTimeUnix = e.endTimeUnix
                }).ToList()
            };

            var response = await networkManager.PostAsync<object>("/expeditions/sync", request);
            return response.success;
        }

        public async Task<bool> ReportClaimAsync(string slotId)
        {
            if (networkManager == null || string.IsNullOrEmpty(slotId)) return false;
            var response = await networkManager.PostAsync<object>("/expeditions/claim", new ExpeditionClaimRequest { slotId = slotId });
            return response.success;
        }
    }
}
