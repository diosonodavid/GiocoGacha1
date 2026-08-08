using GachaGame.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // In-battle HUD controls: speed multiplier buttons (x1/x2/x3) and an auto-battle AI toggle,
    // both delegating to an AutoBattleController bound at battle start.
    public class AutoBattleUI : MonoBehaviour
    {
        [SerializeField] private Button speed1xButton;
        [SerializeField] private Button speed2xButton;
        [SerializeField] private Button speed3xButton;
        [SerializeField] private Button autoBattleToggleButton;
        [SerializeField] private Text autoBattleStateText;

        private AutoBattleController autoBattleController;

        private void Awake()
        {
            if (speed1xButton != null) speed1xButton.onClick.AddListener(() => SetSpeed(1f));
            if (speed2xButton != null) speed2xButton.onClick.AddListener(() => SetSpeed(2f));
            if (speed3xButton != null) speed3xButton.onClick.AddListener(() => SetSpeed(3f));
            if (autoBattleToggleButton != null) autoBattleToggleButton.onClick.AddListener(ToggleAutoBattle);
        }

        public void Bind(AutoBattleController controller)
        {
            autoBattleController = controller;
            RefreshAutoBattleState();
        }

        private void SetSpeed(float multiplier) => autoBattleController?.SetSpeedMultiplier(multiplier);

        private void ToggleAutoBattle()
        {
            if (autoBattleController == null) return;

            autoBattleController.SetAutoBattleEnabled(!autoBattleController.IsAutoBattleEnabled);
            RefreshAutoBattleState();
        }

        private void RefreshAutoBattleState()
        {
            if (autoBattleStateText != null)
                autoBattleStateText.text = autoBattleController != null && autoBattleController.IsAutoBattleEnabled ? "AUTO: ON" : "AUTO: OFF";
        }

        // Resets Time.timeScale so leaving battle doesn't leave the whole game sped up.
        private void OnDestroy()
        {
            if (autoBattleController != null) Time.timeScale = 1f;
        }
    }
}
