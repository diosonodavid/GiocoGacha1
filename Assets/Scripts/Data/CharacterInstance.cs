using System;
using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class CharacterInstance : ICombatant
    {
        public string instanceId;
        public string baseDataId;
        public int currentLevel;
        public int currentExp;
        public int memoryLevel; // duplicate count, 0-6
        public int ascensionLevel; // star tier reached via CharacterAscensionSystem, 0 = base
        public SerializableDictionary<string, GearData> equippedGear = new();

        [NonSerialized] private Dictionary<StatType, float> combatStats = new();
        [NonSerialized] private readonly List<StatModifier> activeStatusEffects = new();
        [NonSerialized] private readonly Dictionary<string, int> skillCooldowns = new();
        [NonSerialized] private int currentHp;
        [NonSerialized] private int maxHp;

        public string Id => instanceId;
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public IReadOnlyDictionary<StatType, float> BaseStats => combatStats;
        public IReadOnlyList<StatModifier> ActiveStatusEffects => activeStatusEffects;
        public bool IsAlive => currentHp > 0;

        // Call once at battle start with the character's fully calculated stats (see StatCalculator).
        public void InitializeCombatState(Dictionary<StatType, float> finalStats)
        {
            combatStats = finalStats ?? new Dictionary<StatType, float>();
            maxHp = Mathf.RoundToInt(combatStats.TryGetValue(StatType.HP, out var hp) ? hp : 0f);
            currentHp = maxHp;
            activeStatusEffects.Clear();
            skillCooldowns.Clear();
        }

        public void ApplyDamage(int amount) => currentHp = Mathf.Max(0, currentHp - Mathf.Max(0, amount));

        public void AddStatusEffect(StatModifier modifier) => activeStatusEffects.Add(modifier);

        public void RemoveStatusEffect(StatType stat) => activeStatusEffects.RemoveAll(m => m.stat == stat);

        public void TickStatusEffectDurations()
        {
            for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeStatusEffects[i];
                effect.durationTurns--;
                if (effect.durationTurns <= 0)
                    activeStatusEffects.RemoveAt(i);
                else
                    activeStatusEffects[i] = effect;
            }
        }

        public int GetSkillCooldown(string skillId) =>
            skillId != null && skillCooldowns.TryGetValue(skillId, out var turns) ? turns : 0;

        public void SetSkillCooldown(string skillId, int turns)
        {
            if (skillId != null) skillCooldowns[skillId] = turns;
        }

        public void TickSkillCooldowns()
        {
            var keys = new List<string>(skillCooldowns.Keys);
            foreach (var key in keys)
            {
                if (skillCooldowns[key] > 0)
                    skillCooldowns[key]--;
            }
        }
    }
}
