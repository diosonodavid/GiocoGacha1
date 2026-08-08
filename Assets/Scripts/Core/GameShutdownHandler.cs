using System;
using System.IO;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Core
{
    // AppBootstrapper.OnApplicationQuit already fires a fire-and-forget ShutdownAsync across
    // every registered service (including NetworkManager's socket disconnect), but "async void
    // OnApplicationQuit" isn't guaranteed to finish before Unity tears the process down. This
    // handler instead uses Application.wantsToQuit, whose bool-returning callback can delay the
    // actual quit until the local save (and socket disconnect) has verifiably completed.
    public class GameShutdownHandler : MonoBehaviour
    {
        [SerializeField] private string saveFileName = "playersave.dat";
        [SerializeField] private string savePassphrase = "GachaGameLocalSave";

        private NetworkManager networkManager;
        private Func<PlayerSaveData> saveDataProvider;
        private bool shutdownComplete;

        private void Awake()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            Application.wantsToQuit += HandleWantsToQuit;
        }

        private void OnDestroy() => Application.wantsToQuit -= HandleWantsToQuit;

        public void SetSaveDataProvider(Func<PlayerSaveData> provider) => saveDataProvider = provider;

        private bool HandleWantsToQuit()
        {
            if (shutdownComplete) return true;

            _ = ShutdownAndQuitAsync();
            return false;
        }

        private async Task ShutdownAndQuitAsync()
        {
            await SaveLocalDataAsync();

            if (networkManager != null) await networkManager.DisconnectSocketAsync();

            shutdownComplete = true;
            Application.Quit();
        }

        private async Task SaveLocalDataAsync()
        {
            var data = saveDataProvider?.Invoke();
            if (data == null) return;

            try
            {
                var saveSystem = new SaveSystem(savePassphrase);
                string path = Path.Combine(Application.persistentDataPath, saveFileName);
                await saveSystem.SaveGameAsync(data, path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{nameof(GameShutdownHandler)}: save failed during shutdown: {ex.Message}");
            }
        }
    }
}
