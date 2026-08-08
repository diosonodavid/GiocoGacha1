using System;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Network
{
    // Periodically pushes the local PlayerSaveData to the backend (POST /player/sync) and can
    // also be triggered manually (e.g. a "Save now" button); NetworkManager already carries the
    // auth token, so this only needs to know what to send and when.
    public class CloudSaveService : MonoBehaviour, IService
    {
        public event Action OnSyncStarted;
        public event Action<bool> OnSyncCompleted; // bool = success

        [SerializeField] private float autoSyncIntervalSeconds = 120f;

        private NetworkManager networkManager;
        private Func<PlayerSaveData> getCurrentSaveData;
        private float syncTimer;
        private bool isSyncing;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            Debug.Log($"{nameof(CloudSaveService)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        // The save data source is injected rather than resolved internally, since ownership of
        // "what the current save looks like" belongs to whatever system assembles PlayerSaveData.
        public void SetSaveDataProvider(Func<PlayerSaveData> provider) => getCurrentSaveData = provider;

        private void Update()
        {
            if (getCurrentSaveData == null) return;

            syncTimer += Time.deltaTime;
            if (syncTimer < autoSyncIntervalSeconds) return;

            syncTimer = 0f;
            _ = SyncNowAsync();
        }

        public async Task<bool> SyncNowAsync()
        {
            if (isSyncing || networkManager == null || getCurrentSaveData == null) return false;

            isSyncing = true;
            OnSyncStarted?.Invoke();

            var response = await networkManager.PostAsync<PlayerSaveData>("/player/sync", getCurrentSaveData());
            bool success = response.success;

            isSyncing = false;
            OnSyncCompleted?.Invoke(success);
            return success;
        }

        public async Task<PlayerSaveData> FetchLatestAsync()
        {
            if (networkManager == null) return null;

            var response = await networkManager.GetAsync<PlayerSaveData>("/player/profile");
            return response.success ? response.data : null;
        }
    }
}
