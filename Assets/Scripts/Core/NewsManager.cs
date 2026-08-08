using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Core
{
    // Holds the announcement list fetched by NewsSyncService and exposes only the ones currently
    // within their start/end window; kept separate from the network concern so UI can bind here
    // without depending on NetworkManager directly.
    public class NewsManager : MonoBehaviour, IService
    {
        public event Action OnAnnouncementsUpdated;

        private readonly List<NewsAnnouncementData> announcements = new();

        public IReadOnlyList<NewsAnnouncementData> ActiveAnnouncements => announcements.FindAll(IsCurrentlyActive);

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(NewsManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void SetAnnouncements(List<NewsAnnouncementData> fetched)
        {
            announcements.Clear();
            if (fetched != null) announcements.AddRange(fetched);
            OnAnnouncementsUpdated?.Invoke();
        }

        private static bool IsCurrentlyActive(NewsAnnouncementData announcement)
        {
            if (announcement == null) return false;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            bool afterStart = announcement.startTimeUnix <= 0 || now >= announcement.startTimeUnix;
            bool beforeEnd = announcement.endTimeUnix <= 0 || now <= announcement.endTimeUnix;
            return afterStart && beforeEnd;
        }
    }
}
