using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Social
{
    [Serializable]
    public class GuildWarOpponentCandidateDto
    {
        public string guildId;
        public string guildName;
        public int totalPower;
    }

    // Asks the server for a pool of guild-war candidates and picks the one closest in total power
    // client-side - mirrors PvPUI's "fetch opponents, let the player/system pick one" flow, but
    // matching a whole guild instead of a single defender.
    public class GuildWarMatchmaking : MonoBehaviour, IService
    {
        public event Action<GuildWarOpponentCandidateDto> OnOpponentMatched;

        private NetworkManager networkManager;

        public GuildWarOpponentCandidateDto MatchedOpponent { get; private set; }

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task<GuildWarOpponentCandidateDto> FindOpponentAsync(int ownGuildTotalPower)
        {
            if (networkManager == null) return null;

            var response = await networkManager.GetAsync<List<GuildWarOpponentCandidateDto>>($"/guildwar/candidates?power={ownGuildTotalPower}");
            if (!response.success || response.data == null || response.data.Count == 0) return null;

            var best = ClosestByPower(response.data, ownGuildTotalPower);
            MatchedOpponent = best;
            OnOpponentMatched?.Invoke(best);
            return best;
        }

        private static GuildWarOpponentCandidateDto ClosestByPower(List<GuildWarOpponentCandidateDto> candidates, int ownPower)
        {
            GuildWarOpponentCandidateDto closest = null;
            int bestDiff = int.MaxValue;

            foreach (var candidate in candidates)
            {
                int diff = Math.Abs(candidate.totalPower - ownPower);
                if (diff >= bestDiff) continue;
                bestDiff = diff;
                closest = candidate;
            }

            return closest;
        }
    }
}
