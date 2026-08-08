using System;
using System.Collections.Generic;
using GachaGame.Data;

namespace GachaGame.Combat
{
    // Tracks each guild member's cumulative damage against a shared HP pool (GuildBossData.totalHp)
    // for the active encounter, and works out a damage-share reward multiplier once it dies. Plain
    // class rather than a MonoBehaviour/IService, like BattleTurnController - this is ephemeral
    // per-encounter state, not a persistent game service. Client-side bookkeeping only; as with
    // AntiCheatManager elsewhere, the server would be the real authority on the shared HP total.
    public class GuildBossManager
    {
        public event Action<long> OnBossDamaged;
        public event Action OnBossDefeated;

        private readonly Dictionary<string, long> damageByMemberId = new();

        public GuildBossData ActiveBoss { get; private set; }
        public long RemainingHp { get; private set; }
        public bool IsDefeated => ActiveBoss != null && RemainingHp <= 0;
        public IReadOnlyDictionary<string, long> DamageByMemberId => damageByMemberId;

        public void StartEncounter(GuildBossData boss)
        {
            ActiveBoss = boss;
            RemainingHp = boss != null ? boss.totalHp : 0;
            damageByMemberId.Clear();
        }

        public long ReportDamage(string memberId, long damage)
        {
            if (ActiveBoss == null || string.IsNullOrEmpty(memberId) || damage <= 0 || IsDefeated) return RemainingHp;

            damageByMemberId[memberId] = GetMemberDamage(memberId) + damage;
            RemainingHp = Math.Max(0, RemainingHp - damage);

            OnBossDamaged?.Invoke(RemainingHp);
            if (RemainingHp <= 0) OnBossDefeated?.Invoke();

            return RemainingHp;
        }

        public long GetMemberDamage(string memberId) =>
            memberId != null && damageByMemberId.TryGetValue(memberId, out var damage) ? damage : 0;

        // Reward scales with each member's share of total damage dealt, so a member who did 10%
        // of the damage gets 10% of the pool - callers apply the resulting share however rewards
        // are granted (gems, guild currency, etc.) for this encounter.
        public float GetMemberRewardShare(string memberId)
        {
            long totalDamageDealt = 0;
            foreach (var damage in damageByMemberId.Values) totalDamageDealt += damage;
            if (totalDamageDealt <= 0) return 0f;

            return GetMemberDamage(memberId) / (float)totalDamageDealt;
        }
    }
}
