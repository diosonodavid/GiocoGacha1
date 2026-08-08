using System;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Inventory
{
    // Owns per-character talent point spending and node unlocking. Prerequisites are resolved by
    // nodeId membership in CharacterInstance.unlockedTalentNodeIds rather than by walking the
    // TalentNodeData asset graph at runtime, so a node with multiple prerequisites just needs all
    // of them present in that list. Where talentPoints are granted (level-up, quests, etc.) is left
    // to callers via GrantTalentPoints - no leveling system exists yet in this codebase to hook into.
    public class TalentTreeManager : MonoBehaviour, IService
    {
        public event Action<CharacterInstance, TalentNodeData> OnNodeUnlocked;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(TalentTreeManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool IsNodeUnlocked(CharacterInstance instance, TalentNodeData node) =>
            instance != null && node != null && instance.unlockedTalentNodeIds.Contains(node.nodeId);

        public bool CanUnlockNode(CharacterInstance instance, TalentNodeData node)
        {
            if (instance == null || node == null) return false;
            if (IsNodeUnlocked(instance, node)) return false;
            if (instance.talentPoints <= 0) return false;

            foreach (var prerequisite in node.prerequisites)
            {
                if (prerequisite != null && !IsNodeUnlocked(instance, prerequisite)) return false;
            }

            return true;
        }

        public bool TryUnlockNode(CharacterInstance instance, TalentNodeData node)
        {
            if (!CanUnlockNode(instance, node)) return false;

            instance.talentPoints--;
            instance.unlockedTalentNodeIds.Add(node.nodeId);
            OnNodeUnlocked?.Invoke(instance, node);
            return true;
        }

        public void GrantTalentPoints(CharacterInstance instance, int amount)
        {
            if (instance == null || amount <= 0) return;
            instance.talentPoints += amount;
        }
    }
}
