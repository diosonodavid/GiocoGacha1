using System;
using System.Collections.Generic;
using System.Linq;
using GachaGame.Data;

namespace GachaGame.Combat
{
    // Tracks each player's cumulative damage against the shared global boss HP pool for the active
    // event and works out rank-based rewards once it dies. Plain class rather than a
    // MonoBehaviour/IService, mirroring GuildBossManager - this is ephemeral per-event state, not a
    // persistent game service. Client-side bookkeeping only; the server would be the real authority
    // on the shared HP total (see WorldBossSyncService).
    public class WorldBossManager
    {
        public event Action<long> OnBossDamaged;
        public event Action OnBossDefeated;

        private readonly Dictionary<string, long> damageByPlayerId = new();

        public WorldBossData ActiveBoss { get; private set; }
        public long RemainingHp { get; private set; }
        public bool IsDefeated => ActiveBoss != null && RemainingHp <= 0;
        public IReadOnlyDictionary<string, long> DamageByPlayerId => damageByPlayerId;

        public void StartEncounter(WorldBossData boss)
        {
            ActiveBoss = boss;
            RemainingHp = boss != null ? boss.totalHP : 0;
            damageByPlayerId.Clear();
        }

        public long ReportDamage(string playerId, long damage)
        {
            if (ActiveBoss == null || string.IsNullOrEmpty(playerId) || damage <= 0 || IsDefeated) return RemainingHp;

            damageByPlayerId[playerId] = GetPlayerDamage(playerId) + damage;
            RemainingHp = Math.Max(0, RemainingHp - damage);

            OnBossDamaged?.Invoke(RemainingHp);
            if (RemainingHp <= 0) OnBossDefeated?.Invoke();

            return RemainingHp;
        }

        public long GetPlayerDamage(string playerId) =>
            playerId != null && damageByPlayerId.TryGetValue(playerId, out var damage) ? damage : 0;

        // 1-based rank by damage dealt (highest first); 0 if the player has dealt no damage yet.
        public int GetPlayerRank(string playerId)
        {
            if (string.IsNullOrEmpty(playerId) || !damageByPlayerId.ContainsKey(playerId)) return 0;

            var ranked = damageByPlayerId.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            int index = ranked.IndexOf(playerId);
            return index >= 0 ? index + 1 : 0;
        }

        public WorldBossRewardTier GetRewardTier(int rank) =>
            ActiveBoss?.rewardTiers.Find(t => rank >= t.minRank && rank <= t.maxRank);
    }
}
