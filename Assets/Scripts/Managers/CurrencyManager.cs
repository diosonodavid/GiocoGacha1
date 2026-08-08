using System;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Managers
{
    public class CurrencyManager : MonoBehaviour, IService
    {
        public event Action<CurrencyType, int> OnCurrencyChanged;

        [SerializeField] private int gold;
        [SerializeField] private int gems;
        [SerializeField] private int stamina;
        [SerializeField] private int maxStamina = 120;

        public int Gold => gold;
        public int Gems => gems;
        public int Stamina => stamina;
        public int MaxStamina => maxStamina;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(CurrencyManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void AddCurrency(CurrencyType type, int amount)
        {
            if (amount <= 0) return;

            switch (type)
            {
                case CurrencyType.Gold: gold += amount; break;
                case CurrencyType.Gems: gems += amount; break;
                case CurrencyType.Stamina: stamina = Mathf.Min(maxStamina, stamina + amount); break;
            }

            OnCurrencyChanged?.Invoke(type, GetBalance(type));
        }

        // Verifies the balance covers the cost before scaling it down; returns false untouched otherwise.
        public bool TrySpendCurrency(CurrencyType type, int amount)
        {
            if (amount <= 0) return true;
            if (GetBalance(type) < amount) return false;

            switch (type)
            {
                case CurrencyType.Gold: gold -= amount; break;
                case CurrencyType.Gems: gems -= amount; break;
                case CurrencyType.Stamina: stamina -= amount; break;
            }

            OnCurrencyChanged?.Invoke(type, GetBalance(type));
            return true;
        }

        public void IncreaseMaxStamina(int amount)
        {
            if (amount <= 0) return;
            maxStamina += amount;
            OnCurrencyChanged?.Invoke(CurrencyType.Stamina, stamina);
        }

        // Passive regen: 1 Stamina every GameConstants.StaminaRegenIntervalSeconds, computed from
        // the elapsed time since the save's last stamina timestamp. Advances the timestamp only by
        // the whole intervals consumed, so partial progress toward the next point carries over.
        public int ApplyPassiveStaminaRegen(ref long lastStaminaUpdateUnix)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsedSeconds = Math.Max(0, now - lastStaminaUpdateUnix);
            int pointsToRegen = (int)(elapsedSeconds / GameConstants.StaminaRegenIntervalSeconds);
            if (pointsToRegen <= 0) return 0;

            int actualRegen = Mathf.Min(pointsToRegen, maxStamina - stamina);
            lastStaminaUpdateUnix += pointsToRegen * (long)GameConstants.StaminaRegenIntervalSeconds;

            if (actualRegen > 0) AddCurrency(CurrencyType.Stamina, actualRegen);
            return actualRegen;
        }

        public int GetBalance(CurrencyType type) => type switch
        {
            CurrencyType.Gold => gold,
            CurrencyType.Gems => gems,
            CurrencyType.Stamina => stamina,
            _ => 0
        };
    }
}
