using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    [Serializable]
    public class ScheduledNotification
    {
        public string id;
        public string title;
        public string body;
        public long fireTimeUnix;
    }

    // Tracks locally-scheduled notifications (stamina full, daily reset) in a platform-agnostic
    // ledger. Actual OS-level dispatch needs the com.unity.mobile.notifications package, which
    // isn't installed in this project (see Packages/manifest.json) - until it's added, scheduling
    // only records state and logs, so this compiles cleanly instead of referencing a missing API.
    public class NotificationManager : MonoBehaviour, IService
    {
        private readonly Dictionary<string, ScheduledNotification> pending = new();

        public IReadOnlyCollection<ScheduledNotification> Pending => pending.Values;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(NotificationManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void ScheduleNotification(string id, string title, string body, DateTime fireTimeUtc)
        {
            if (string.IsNullOrEmpty(id)) return;

            pending[id] = new ScheduledNotification
            {
                id = id,
                title = title,
                body = body,
                fireTimeUnix = new DateTimeOffset(fireTimeUtc).ToUnixTimeSeconds()
            };

            Debug.Log($"[{nameof(NotificationManager)}] Scheduled '{id}' for {fireTimeUtc:u} (OS dispatch pending mobile-notifications package).");
        }

        public void ScheduleStaminaFullNotification(int minutesUntilFull) =>
            ScheduleNotification("stamina_full", "Stamina Full!", "Your stamina is ready to use.", DateTime.UtcNow.AddMinutes(minutesUntilFull));

        public void ScheduleDailyResetNotification(DateTime resetTimeUtc) =>
            ScheduleNotification("daily_reset", "Daily Reset", "New daily quests and dungeons are available.", resetTimeUtc);

        public void CancelNotification(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            pending.Remove(id);
        }

        public void CancelAll() => pending.Clear();
    }
}
