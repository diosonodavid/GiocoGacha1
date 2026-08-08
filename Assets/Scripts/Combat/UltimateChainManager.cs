using System;
using System.Collections.Generic;
using GachaGame.Data;

namespace GachaGame.Combat
{
    public class QueuedUltimate
    {
        public CharacterInstance caster;
        public SkillData skill;
    }

    // Lets multiple party members queue their Ultimate so they resolve together as one combined
    // chain attack instead of independently. Decision/queueing only - actual combined damage
    // resolution is left to the OnChainTriggered subscriber, so SkillExecutionEngine stays untouched.
    public class UltimateChainManager
    {
        private readonly List<QueuedUltimate> queuedUltimates = new();
        private readonly int requiredCountToTrigger;

        public event Action<IReadOnlyList<QueuedUltimate>> OnChainTriggered;

        public UltimateChainManager(int requiredCountToTrigger = 2)
        {
            this.requiredCountToTrigger = Math.Max(1, requiredCountToTrigger);
        }

        public IReadOnlyList<QueuedUltimate> QueuedUltimates => queuedUltimates;

        public void QueueUltimate(CharacterInstance caster, SkillData skill)
        {
            if (caster == null || skill == null) return;

            queuedUltimates.Add(new QueuedUltimate { caster = caster, skill = skill });
            if (queuedUltimates.Count >= requiredCountToTrigger)
                TriggerChain();
        }

        public void ClearQueue() => queuedUltimates.Clear();

        private void TriggerChain()
        {
            var triggered = new List<QueuedUltimate>(queuedUltimates);
            queuedUltimates.Clear();
            OnChainTriggered?.Invoke(triggered);
        }
    }
}
