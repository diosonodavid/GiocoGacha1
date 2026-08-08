using System.Collections.Generic;
using GachaGame.Data;

namespace GachaGame.Core
{
    // Minimal surface any battle participant must expose. Kept small so future combatant
    // types (e.g. enemies) can implement it without depending on the full save-data model.
    public interface ICombatant
    {
        string Id { get; }
        int CurrentHp { get; }
        int MaxHp { get; }
        IReadOnlyDictionary<StatType, float> BaseStats { get; }
        IReadOnlyList<StatModifier> ActiveStatusEffects { get; }
        bool IsAlive { get; }

        void ApplyDamage(int amount);
        void AddStatusEffect(StatModifier modifier);
        void RemoveStatusEffect(StatType stat);
        void TickStatusEffectDurations();
    }
}
