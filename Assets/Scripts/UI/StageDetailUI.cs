using System;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Shows the selected stage's enemy roster and stamina cost, and gates the "start battle"
    // button on the player actually having enough stamina to pay for it.
    public class StageDetailUI : UIController
    {
        [SerializeField] private Text stageNameText;
        [SerializeField] private Text staminaCostText;
        [SerializeField] private Transform enemyListContainer;
        [SerializeField] private GameObject enemyEntryPrefab;
        [SerializeField] private Button startBattleButton;

        private CurrencyManager currencyManager;
        private StageData currentStage;

        public event Action<StageData> OnBattleRequested;

        private void Awake()
        {
            if (startBattleButton != null)
                startBattleButton.onClick.AddListener(RequestBattle);
        }

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out currencyManager);
            RefreshDetails();
        }

        public void SetStage(StageData stage)
        {
            currentStage = stage;
            RefreshDetails();
        }

        private void RefreshDetails()
        {
            if (currentStage == null) return;

            if (stageNameText != null) stageNameText.text = currentStage.stageName;

            RefreshStaminaCost();
            RefreshEnemyList();
        }

        private void RefreshStaminaCost()
        {
            if (staminaCostText != null)
            {
                staminaCostText.text = currencyManager != null
                    ? $"{currentStage.staminaCost}/{currencyManager.Stamina}"
                    : currentStage.staminaCost.ToString();
            }

            if (startBattleButton != null)
                startBattleButton.interactable = currencyManager == null || currencyManager.Stamina >= currentStage.staminaCost;
        }

        private void RefreshEnemyList()
        {
            if (enemyListContainer == null || enemyEntryPrefab == null) return;

            for (int i = enemyListContainer.childCount - 1; i >= 0; i--)
                Destroy(enemyListContainer.GetChild(i).gameObject);

            foreach (var enemy in currentStage.listNemici)
            {
                var go = Instantiate(enemyEntryPrefab, enemyListContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = enemy != null ? enemy.characterName : string.Empty;
            }
        }

        private void RequestBattle()
        {
            if (currentStage == null) return;
            if (currencyManager != null && currencyManager.Stamina < currentStage.staminaCost) return;

            OnBattleRequested?.Invoke(currentStage);
        }
    }
}
