using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using NUnit.Framework;

namespace GachaGame.Tests
{
    public class CombatTests
    {
        private class FakeCombatant : ICombatant
        {
            private readonly List<StatModifier> statusEffects = new();
            private readonly Dictionary<StatType, float> stats;

            public FakeCombatant(string id, float spd, int maxHp = 1000)
            {
                Id = id;
                MaxHp = maxHp;
                CurrentHp = maxHp;
                stats = new Dictionary<StatType, float> { { StatType.SPD, spd } };
            }

            public string Id { get; }
            public int CurrentHp { get; private set; }
            public int MaxHp { get; }
            public IReadOnlyDictionary<StatType, float> BaseStats => stats;
            public IReadOnlyList<StatModifier> ActiveStatusEffects => statusEffects;
            public bool IsAlive => CurrentHp > 0;

            public void ApplyDamage(int amount) => CurrentHp = System.Math.Max(0, CurrentHp - amount);
            public void AddStatusEffect(StatModifier modifier) => statusEffects.Add(modifier);
            public void RemoveStatusEffect(StatType stat) => statusEffects.RemoveAll(m => m.stat == stat);

            public void TickStatusEffectDurations()
            {
                for (int i = statusEffects.Count - 1; i >= 0; i--)
                {
                    var effect = statusEffects[i];
                    effect.durationTurns--;
                    if (effect.durationTurns <= 0) statusEffects.RemoveAt(i);
                    else statusEffects[i] = effect;
                }
            }
        }

        [Test]
        public void DamageCalculator_HigherDefenseReducesDamage()
        {
            var lowDef = DamageCalculator.CalculateDamage(1000f, 0f, 1f, ElementType.Fire, ElementType.Fire, false, 150f, new System.Random(1));
            var highDef = DamageCalculator.CalculateDamage(1000f, 200f, 1f, ElementType.Fire, ElementType.Fire, false, 150f, new System.Random(1));

            Assert.Greater(lowDef.damageAmount, highDef.damageAmount);
        }

        [Test]
        public void DamageCalculator_ElementalWeaknessDealsBonusDamage()
        {
            var neutral = DamageCalculator.CalculateDamage(1000f, 100f, 1f, ElementType.Fire, ElementType.Fire, false, 150f, new System.Random(1));
            var weakness = DamageCalculator.CalculateDamage(1000f, 100f, 1f, ElementType.Fire, ElementType.Grass, false, 150f, new System.Random(1));

            Assert.IsTrue(weakness.isWeakness);
            Assert.Greater(weakness.damageAmount, neutral.damageAmount);
        }

        [Test]
        public void DamageCalculator_ElementalDisadvantageReducesDamage()
        {
            var neutral = DamageCalculator.CalculateDamage(1000f, 100f, 1f, ElementType.Grass, ElementType.Light, false, 150f, new System.Random(1));
            var disadvantage = DamageCalculator.CalculateDamage(1000f, 100f, 1f, ElementType.Grass, ElementType.Fire, false, 150f, new System.Random(1));

            Assert.IsFalse(disadvantage.isWeakness);
            Assert.Less(disadvantage.damageAmount, neutral.damageAmount);
        }

        [Test]
        public void DamageCalculator_LightDarkReciprocalAppliesBonus()
        {
            var neutral = DamageCalculator.CalculateDamage(1000f, 100f, 1f, ElementType.Light, ElementType.Fire, false, 150f, new System.Random(1));
            var weakness = DamageCalculator.CalculateDamage(1000f, 100f, 1f, ElementType.Light, ElementType.Dark, false, 150f, new System.Random(1));

            Assert.IsTrue(weakness.isWeakness);
            Assert.Greater(weakness.damageAmount, neutral.damageAmount);
        }

        [Test]
        public void DamageCalculator_CriticalHitDealsMoreDamage()
        {
            var normal = DamageCalculator.CalculateDamage(1000f, 100f, 1f, ElementType.Fire, ElementType.Fire, false, 150f, new System.Random(1));
            var critical = DamageCalculator.CalculateDamage(1000f, 100f, 1f, ElementType.Fire, ElementType.Fire, true, 150f, new System.Random(1));

            Assert.Greater(critical.damageAmount, normal.damageAmount);
        }

        [Test]
        public void TurnManager_FasterCombatantActsMoreOften()
        {
            var fast = new FakeCombatant("fast", 200f);
            var slow = new FakeCombatant("slow", 100f);

            var turnManager = new TurnManager();
            turnManager.Initialize(new ICombatant[] { fast, slow });

            int fastActions = 0;
            int slowActions = 0;
            for (int i = 0; i < 20; i++)
            {
                var actor = turnManager.GetNextActor();
                if (actor == fast) fastActions++;
                else if (actor == slow) slowActions++;
            }

            Assert.Greater(fastActions, slowActions);
        }

        [Test]
        public void TurnManager_RemovedCombatantNeverActs()
        {
            var a = new FakeCombatant("a", 100f);
            var b = new FakeCombatant("b", 100f);

            var turnManager = new TurnManager();
            turnManager.Initialize(new ICombatant[] { a, b });
            turnManager.RemoveCombatant(a);

            for (int i = 0; i < 10; i++)
                Assert.AreNotEqual(a, turnManager.GetNextActor());
        }

        [Test]
        public void TurnManager_DeadCombatantIsSkipped()
        {
            var alive = new FakeCombatant("alive", 100f);
            var dying = new FakeCombatant("dying", 100f);
            dying.ApplyDamage(dying.MaxHp);

            var turnManager = new TurnManager();
            turnManager.Initialize(new ICombatant[] { alive, dying });

            for (int i = 0; i < 5; i++)
                Assert.AreEqual(alive, turnManager.GetNextActor());
        }
    }
}
