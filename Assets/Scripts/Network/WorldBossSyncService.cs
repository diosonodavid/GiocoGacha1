using System;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Network
{
    [Serializable]
    public class WorldBossDamageReportRequest
    {
        public string bossId;
        public long damage;
    }

    [Serializable]
    public class WorldBossStateResponse
    {
        public long remainingHp;
        public long totalHp;
    }

    // Pushes damage dealt to the global boss up to the backend and pulls the authoritative
    // remaining-HP total back down. WorldBossManager is ephemeral per-encounter state (see its own
    // comment) rather than a registered IService, so callers pass the relevant boss id in explicitly
    // instead of this service resolving a WorldBossManager from ServiceLocator.
    public class WorldBossSyncService : MonoBehaviour, IService
    {
        private NetworkManager networkManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            Debug.Log($"{nameof(WorldBossSyncService)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task<bool> ReportDamageAsync(string bossId, long damage)
        {
            if (networkManager == null) return false;

            var response = await networkManager.PostAsync<object>("/worldboss/damage",
                new WorldBossDamageReportRequest { bossId = bossId, damage = damage });
            return response.success;
        }

        public async Task<WorldBossStateResponse> FetchBossStateAsync(string bossId)
        {
            if (networkManager == null) return null;

            var response = await networkManager.GetAsync<WorldBossStateResponse>($"/worldboss/state?bossId={bossId}");
            return response.success ? response.data : null;
        }
    }
}
