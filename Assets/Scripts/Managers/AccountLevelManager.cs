using System;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Managers
{
    public class AccountLevelManager : MonoBehaviour, IService
    {
        public event Action<int> OnLevelUp;

        [SerializeField] private int level = 1;
        [SerializeField] private int exp;

        public int Level => level;
        public int Exp => exp;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(AccountLevelManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void LoadState(int savedLevel, int savedExp)
        {
            level = Mathf.Max(1, savedLevel);
            exp = Mathf.Max(0, savedExp);
        }

        // Levels up as many times as the added EXP covers, growing MaxStamina on the currency
        // manager each time.
        public void AddExp(int amount, CurrencyManager currencyManager = null)
        {
            if (amount <= 0) return;
            exp += amount;

            while (exp >= GetExpRequiredForLevel(level))
            {
                exp -= GetExpRequiredForLevel(level);
                level++;
                currencyManager?.IncreaseMaxStamina(GameConstants.MaxStaminaGainPerLevel);
                OnLevelUp?.Invoke(level);
            }
        }

        public static int GetExpRequiredForLevel(int level) => 100 * level;
    }
}
