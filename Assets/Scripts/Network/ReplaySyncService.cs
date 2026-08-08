using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Combat;
using GachaGame.Core;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Network
{
    [Serializable]
    public class ReplaySummaryDto
    {
        public string replayId;
        public string ownerName;
        public int rank;
    }

    // Downloads BattleReplayData for the leaderboard's top matches - mirrors WorldBossSyncService's
    // shape (a thin NetworkManager wrapper) but pulls replay payloads instead of pushing damage.
    public class ReplaySyncService : MonoBehaviour, IService
    {
        private NetworkManager networkManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task<List<ReplaySummaryDto>> FetchTopReplaysAsync()
        {
            if (networkManager == null) return new List<ReplaySummaryDto>();

            var response = await networkManager.GetAsync<List<ReplaySummaryDto>>("/replays/top");
            return response.success && response.data != null ? response.data : new List<ReplaySummaryDto>();
        }

        public async Task<BattleReplayData> DownloadReplayAsync(string replayId)
        {
            if (networkManager == null || string.IsNullOrEmpty(replayId)) return null;

            var response = await networkManager.GetAsync<BattleReplayData>($"/replays/{replayId}");
            return response.success ? response.data : null;
        }
    }
}
