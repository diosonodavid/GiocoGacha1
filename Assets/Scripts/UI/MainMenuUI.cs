using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Reads live values from the currency/account services (via ServiceLocator) and keeps the
    // HUD in sync by subscribing to their change events rather than polling every frame.
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Text goldText;
        [SerializeField] private Text gemsText;
        [SerializeField] private Text staminaText;
        [SerializeField] private Text accountLevelText;
        [SerializeField] private Slider accountExpBar;
        [SerializeField] private Text playerNameText;

        [SerializeField] private GameObject gachaPanel;
        [SerializeField] private GameObject battlePanel;
        [SerializeField] private GameObject inventoryPanel;

        private CurrencyManager currencyManager;
        private AccountLevelManager accountLevelManager;

        private void OnEnable()
        {
            if (!ServiceLocator.Instance.TryGet(out currencyManager) ||
                !ServiceLocator.Instance.TryGet(out accountLevelManager))
            {
                Debug.LogWarning($"{nameof(MainMenuUI)} could not resolve required services.", this);
                return;
            }

            currencyManager.OnCurrencyChanged += HandleCurrencyChanged;
            accountLevelManager.OnLevelUp += HandleLevelUp;

            RefreshAll();
        }

        private void OnDisable()
        {
            if (currencyManager != null) currencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
            if (accountLevelManager != null) accountLevelManager.OnLevelUp -= HandleLevelUp;
        }

        public void RefreshAll()
        {
            if (currencyManager == null || accountLevelManager == null) return;

            SetCurrencyText(CurrencyType.Gold, currencyManager.Gold);
            SetCurrencyText(CurrencyType.Gems, currencyManager.Gems);
            SetCurrencyText(CurrencyType.Stamina, currencyManager.Stamina);
            RefreshAccountLevel();
        }

        public void SetPlayerName(string playerName)
        {
            if (playerNameText != null) playerNameText.text = playerName;
        }

        // Wired to the HUD nav buttons' OnClick() in the Inspector; the Debug.Log lets each
        // button be verified from the Inspector/Console before its panel reference is hooked up.
        public void OpenGachaPanel()
        {
            Debug.Log($"{nameof(MainMenuUI)}.{nameof(OpenGachaPanel)} called.");
            ShowOnlyPanel(gachaPanel);
        }

        public void OpenBattlePanel()
        {
            Debug.Log($"{nameof(MainMenuUI)}.{nameof(OpenBattlePanel)} called.");
            ShowOnlyPanel(battlePanel);
        }

        public void OpenInventoryPanel()
        {
            Debug.Log($"{nameof(MainMenuUI)}.{nameof(OpenInventoryPanel)} called.");
            ShowOnlyPanel(inventoryPanel);
        }

        private void ShowOnlyPanel(GameObject panelToShow)
        {
            if (gachaPanel != null) gachaPanel.SetActive(gachaPanel == panelToShow);
            if (battlePanel != null) battlePanel.SetActive(battlePanel == panelToShow);
            if (inventoryPanel != null) inventoryPanel.SetActive(inventoryPanel == panelToShow);
        }

        private void HandleCurrencyChanged(CurrencyType type, int newBalance) => SetCurrencyText(type, newBalance);

        private void HandleLevelUp(int newLevel) => RefreshAccountLevel();

        private void RefreshAccountLevel()
        {
            if (accountLevelText != null)
                accountLevelText.text = $"Lv. {accountLevelManager.Level}";

            if (accountExpBar != null)
            {
                int required = AccountLevelManager.GetExpRequiredForLevel(accountLevelManager.Level);
                accountExpBar.maxValue = required;
                accountExpBar.value = accountLevelManager.Exp;
            }
        }

        private void SetCurrencyText(CurrencyType type, int amount)
        {
            switch (type)
            {
                case CurrencyType.Gold when goldText != null: goldText.text = amount.ToString("N0"); break;
                case CurrencyType.Gems when gemsText != null: gemsText.text = amount.ToString("N0"); break;
                case CurrencyType.Stamina when staminaText != null:
                    staminaText.text = $"{amount}/{currencyManager.MaxStamina}";
                    break;
            }
        }
    }
}
