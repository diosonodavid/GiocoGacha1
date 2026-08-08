using GachaGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Low-power toggle screen, meant to be surfaced during prolonged auto-battle farming - lets the
    // player force BatterySaverMode on/off instead of waiting for its idle timeout.
    public class BatterySaverUI : UIController
    {
        [SerializeField] private Toggle batterySaverToggle;
        [SerializeField] private Text statusText;

        private BatterySaverMode batterySaverMode;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out batterySaverMode);

            if (batterySaverMode != null)
            {
                batterySaverMode.OnBatterySaverToggled += HandleToggled;
                if (batterySaverToggle != null) batterySaverToggle.isOn = batterySaverMode.IsActive;
            }

            RefreshStatus();
        }

        protected override void OnHidden()
        {
            if (batterySaverMode != null) batterySaverMode.OnBatterySaverToggled -= HandleToggled;
        }

        public void HandleTogglePressed(bool isOn) => batterySaverMode?.SetActive(isOn);

        private void HandleToggled(bool active)
        {
            if (batterySaverToggle != null) batterySaverToggle.isOn = active;
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (statusText == null) return;
            statusText.text = batterySaverMode != null && batterySaverMode.IsActive ? "Battery Saver ON" : "Battery Saver OFF";
        }
    }
}
