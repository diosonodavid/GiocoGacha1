using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Network
{
    [Serializable]
    public class NewsAnnouncementListResponse
    {
        public List<NewsAnnouncementData> announcements;
    }

    // Downloads the active announcement/banner list from the backend at app open and feeds it into
    // NewsManager; kept separate so the fetched-state holder doesn't depend on NetworkManager directly.
    public class NewsSyncService : MonoBehaviour, IService
    {
        private NetworkManager networkManager;
        private NewsManager newsManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            ServiceLocator.Instance.TryGet(out newsManager);
            Debug.Log($"{nameof(NewsSyncService)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task<bool> FetchAnnouncementsAsync()
        {
            if (networkManager == null || newsManager == null) return false;

            var response = await networkManager.GetAsync<NewsAnnouncementListResponse>("/news/announcements");
            if (!response.success || response.data == null) return false;

            newsManager.SetAnnouncements(response.data.announcements);
            return true;
        }
    }
}
