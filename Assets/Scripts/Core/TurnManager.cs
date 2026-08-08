using System.Collections.Generic;
using GachaGame.Data;

namespace GachaGame.Core
{
    // Conditional Turn-Based queue: each combatant accumulates an Action Gauge of
    // 10000 / SPD, and whoever reaches the lowest cumulative time acts next.
    public class TurnManager
    {
        public const float ActionGaugeBase = 10000f;

        private class TurnEntry
        {
            public ICombatant Combatant;
            public float CumulativeTime;
        }

        private readonly List<TurnEntry> queue = new();

        public void Initialize(IEnumerable<ICombatant> combatants)
        {
            queue.Clear();
            foreach (var combatant in combatants)
            {
                if (combatant == null || !combatant.IsAlive) continue;
                queue.Add(new TurnEntry { Combatant = combatant, CumulativeTime = CalculateActionValue(combatant) });
            }
        }

        public void RemoveCombatant(ICombatant combatant)
        {
            queue.RemoveAll(entry => entry.Combatant == combatant);
        }

        public ICombatant GetNextActor()
        {
            queue.RemoveAll(entry => !entry.Combatant.IsAlive);
            if (queue.Count == 0) return null;

            var next = queue[0];
            for (int i = 1; i < queue.Count; i++)
            {
                if (queue[i].CumulativeTime < next.CumulativeTime)
                    next = queue[i];
            }

            next.CumulativeTime += CalculateActionValue(next.Combatant);
            return next.Combatant;
        }

        public static float CalculateActionValue(ICombatant combatant)
        {
            float spd = combatant.BaseStats != null && combatant.BaseStats.TryGetValue(StatType.SPD, out var value)
                ? value
                : 0f;
            return ActionGaugeBase / (spd > 0f ? spd : 1f);
        }
    }
}
