using System;
using System.Threading.Tasks;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Core
{
    // Owns only the season's active/inactive lifecycle and broadcasts the reset moment - it
    // deliberately doesn't reach into ArenaManager/GuildWarManager/battle-pass-style systems
    // itself to clear their state. Each of those is expected to subscribe to OnSeasonReset and
    // reset its own data, the same caller/subscriber-driven restraint used throughout this
    // codebase (e.g. BossPhaseController, BatterySaverMode.NotifyPlayerActivity).
    public class SeasonManager : MonoBehaviour, IService
    {
        [SerializeField] private SeasonData currentSeason;

        public event Action<SeasonData> OnSeasonStarted;
        public event Action<SeasonData> OnSeasonReset;

        private bool hasResetThisSeason;

        public SeasonData CurrentSeason => currentSeason;

        public bool IsActive
        {
            get
            {
                if (currentSeason == null) return false;
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return now >= currentSeason.startUnix && now < currentSeason.endUnix;
            }
        }

        public long SecondsRemaining
        {
            get
            {
                if (currentSeason == null) return 0;
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return Math.Max(0, currentSeason.endUnix - now);
            }
        }

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(SeasonManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void SetSeason(SeasonData season)
        {
            currentSeason = season;
            hasResetThisSeason = false;
            OnSeasonStarted?.Invoke(season);
        }

        private void Update()
        {
            if (currentSeason == null || hasResetThisSeason) return;
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() < currentSeason.endUnix) return;

            hasResetThisSeason = true;
            OnSeasonReset?.Invoke(currentSeason);
        }
    }
}
