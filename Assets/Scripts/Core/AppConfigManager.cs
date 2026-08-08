using System;
using System.Threading.Tasks;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Core
{
    [Serializable]
    public class RemoteAppConfig
    {
        public bool isUnderMaintenance;
        public string maintenanceMessage;
        public string minimumClientVersion;
    }

    // Fetches remote config (maintenance flag, minimum supported client version) once at startup
    // and on demand, so MaintenanceUI and an update-required gate can react without either of
    // them talking to NetworkManager directly.
    public class AppConfigManager : MonoBehaviour, IService
    {
        public event Action<RemoteAppConfig> OnConfigRefreshed;

        private NetworkManager networkManager;

        public RemoteAppConfig CurrentConfig { get; private set; } = new();
        public bool IsUnderMaintenance => CurrentConfig.isUnderMaintenance;

        public async Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            await RefreshConfigAsync();

            Debug.Log($"{nameof(AppConfigManager)} initialized.");
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task RefreshConfigAsync()
        {
            if (networkManager == null) return;

            var response = await networkManager.GetAsync<RemoteAppConfig>("/config/app");
            if (!response.success || response.data == null) return;

            CurrentConfig = response.data;
            OnConfigRefreshed?.Invoke(CurrentConfig);
        }

        // True if the running client's version is older than the server's required minimum.
        public bool IsClientVersionOutdated()
        {
            if (string.IsNullOrEmpty(CurrentConfig.minimumClientVersion)) return false;
            return CompareVersions(Application.version, CurrentConfig.minimumClientVersion) < 0;
        }

        private static int CompareVersions(string current, string minimum)
        {
            if (Version.TryParse(current, out var currentVersion) && Version.TryParse(minimum, out var minimumVersion))
                return currentVersion.CompareTo(minimumVersion);

            return string.CompareOrdinal(current, minimum);
        }
    }
}
