using System;
using UnityEngine;

namespace GachaGame.Combat
{
    // Ephemeral per-battle state (mirrors EnemyWaveManager/GuildBossManager): tracks consecutive
    // successful hits and derives a progressive damage multiplier, capped at maxMultiplier.
    public class ComboSystem
    {
        private readonly float multiplierPerHit;
        private readonly float maxMultiplier;

        public int ComboCount { get; private set; }

        public event Action<int> OnComboChanged;
        public event Action OnComboReset;

        public ComboSystem(float multiplierPerHit = 0.05f, float maxMultiplier = 2f)
        {
            this.multiplierPerHit = multiplierPerHit;
            this.maxMultiplier = maxMultiplier;
        }

        public float CurrentDamageMultiplier => Mathf.Min(maxMultiplier, 1f + ComboCount * multiplierPerHit);

        public void RegisterHit()
        {
            ComboCount++;
            OnComboChanged?.Invoke(ComboCount);
        }

        public void ResetCombo()
        {
            if (ComboCount == 0) return;
            ComboCount = 0;
            OnComboReset?.Invoke();
        }
    }
}
