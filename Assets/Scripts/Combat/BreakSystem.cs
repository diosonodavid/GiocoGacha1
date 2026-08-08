using System;
using System.Collections.Generic;
using GachaGame.Core;
using UnityEngine;

namespace GachaGame.Combat
{
    public class BreakGaugeState
    {
        public float currentGauge;
        public float maxGauge;
        public bool isBroken;
    }

    // Posture/shield mechanic tracked separately from HP: enemies accumulate break damage until
    // their gauge hits zero, becoming Broken (stunned/vulnerable) until ResetGauge is called.
    public class BreakSystem
    {
        private readonly Dictionary<ICombatant, BreakGaugeState> gauges = new();

        public event Action<ICombatant> OnEnemyBroken;
        public event Action<ICombatant, float> OnBreakGaugeChanged;

        public void RegisterEnemy(ICombatant enemy, float maxGauge)
        {
            if (enemy == null || maxGauge <= 0) return;
            gauges[enemy] = new BreakGaugeState { currentGauge = maxGauge, maxGauge = maxGauge, isBroken = false };
        }

        public bool IsBroken(ICombatant enemy) => gauges.TryGetValue(enemy, out var state) && state.isBroken;

        public float GetGaugeRatio(ICombatant enemy) =>
            gauges.TryGetValue(enemy, out var state) && state.maxGauge > 0 ? state.currentGauge / state.maxGauge : 0f;

        public void ApplyBreakDamage(ICombatant enemy, float amount)
        {
            if (enemy == null || amount <= 0 || !gauges.TryGetValue(enemy, out var state) || state.isBroken) return;

            state.currentGauge = Mathf.Max(0f, state.currentGauge - amount);
            OnBreakGaugeChanged?.Invoke(enemy, state.currentGauge / state.maxGauge);

            if (state.currentGauge <= 0f)
            {
                state.isBroken = true;
                OnEnemyBroken?.Invoke(enemy);
            }
        }

        // Call once the stun window ends so the gauge can be filled again.
        public void ResetGauge(ICombatant enemy)
        {
            if (!gauges.TryGetValue(enemy, out var state)) return;
            state.currentGauge = state.maxGauge;
            state.isBroken = false;
        }
    }
}
