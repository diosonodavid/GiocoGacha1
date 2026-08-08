using System.Threading.Tasks;
using GachaGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Full-screen block shown while AppConfigManager reports the server under maintenance;
    // subscribes to OnConfigRefreshed so it can show/hide itself as soon as a refresh changes the
    // flag, without the caller needing to poll.
    public class MaintenanceUI : UIController
    {
        [SerializeField] private Text messageText;
        [SerializeField] private Button retryButton;

        private AppConfigManager appConfigManager;

        private void Awake()
        {
            if (retryButton != null) retryButton.onClick.AddListener(() => _ = RetryNowAsync());
        }

        protected override void OnShown()
        {
            if (ServiceLocator.Instance.TryGet(out appConfigManager))
                appConfigManager.OnConfigRefreshed += HandleConfigRefreshed;

            RefreshMessage();
        }

        protected override void OnHidden()
        {
            if (appConfigManager != null)
                appConfigManager.OnConfigRefreshed -= HandleConfigRefreshed;
        }

        public async Task RetryNowAsync()
        {
            if (appConfigManager == null) return;

            await appConfigManager.RefreshConfigAsync();
            if (!appConfigManager.IsUnderMaintenance) Hide();
        }

        private void HandleConfigRefreshed(RemoteAppConfig config)
        {
            if (!config.isUnderMaintenance) Hide();
            else RefreshMessage();
        }

        private void RefreshMessage()
        {
            if (messageText != null && appConfigManager != null)
                messageText.text = appConfigManager.CurrentConfig.maintenanceMessage;
        }
    }
}
