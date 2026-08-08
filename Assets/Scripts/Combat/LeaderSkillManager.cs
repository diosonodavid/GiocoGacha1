using System.Collections.Generic;
using GachaGame.Data;

namespace GachaGame.Combat
{
    // Folds a LeaderSkillData's team-wide bonus into each teammate's already-calculated stat
    // totals right before battle starts. Operates on plain Dictionary<StatType, float> (the same
    // shape StatCalculator.CalculateFinalStats produces) rather than CharacterInstance directly,
    // so the caller decides exactly when to lock stats in via CharacterInstance.InitializeCombatState.
    public static class LeaderSkillManager
    {
        public static void ApplyLeaderSkill(LeaderSkillData leaderSkill, IEnumerable<Dictionary<StatType, float>> finalStatsByMember)
        {
            if (leaderSkill == null || finalStatsByMember == null) return;

            foreach (var memberStats in finalStatsByMember)
                ApplyBonusesToStats(memberStats, leaderSkill);
        }

        // Convenience overload that also finishes the job by calling InitializeCombatState, for
        // callers that don't need to inspect the boosted stats before locking them in.
        public static void ApplyLeaderSkillAndInitialize(LeaderSkillData leaderSkill, IEnumerable<(CharacterInstance member, Dictionary<StatType, float> finalStats)> team)
        {
            if (team == null) return;

            foreach (var (member, finalStats) in team)
            {
                if (member == null || finalStats == null) continue;
                if (leaderSkill != null) ApplyBonusesToStats(finalStats, leaderSkill);
                member.InitializeCombatState(finalStats);
            }
        }

        private static void ApplyBonusesToStats(Dictionary<StatType, float> stats, LeaderSkillData leaderSkill)
        {
            if (stats == null) return;

            foreach (var bonus in leaderSkill.statBonuses)
            {
                float baseValue = stats.TryGetValue(bonus.stat, out var value) ? value : 0f;
                float delta = bonus.isPercentage ? baseValue * (bonus.value / 100f) : bonus.value;
                stats[bonus.stat] = baseValue + delta;
            }
        }
    }
}
