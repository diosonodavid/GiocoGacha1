using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Core
{
    [Serializable]
    public class CalendarEvent
    {
        public string eventId;
        public string eventName;
        public long startTimeUnix;
        public long endTimeUnix;
    }

    // Holds the fetched weekly/monthly event calendar and computes countdowns to the next daily
    // and weekly reset, reusing ShopManager's fixed-interval reset convention (GameConstants) rather
    // than tracking its own separate reset timestamps.
    public class EventCalendarManager : MonoBehaviour, IService
    {
        public event Action OnCalendarUpdated;

        private readonly List<CalendarEvent> events = new();

        public IReadOnlyList<CalendarEvent> AllEvents => events;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(EventCalendarManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void SetEvents(List<CalendarEvent> fetchedEvents)
        {
            events.Clear();
            if (fetchedEvents != null) events.AddRange(fetchedEvents);
            OnCalendarUpdated?.Invoke();
        }

        public IEnumerable<CalendarEvent> GetActiveEvents()
        {
            long now = Now();
            return events.Where(e => now >= e.startTimeUnix && now < e.endTimeUnix);
        }

        public IEnumerable<CalendarEvent> GetUpcomingEvents()
        {
            long now = Now();
            return events.Where(e => e.startTimeUnix > now).OrderBy(e => e.startTimeUnix);
        }

        public TimeSpan GetTimeUntilDailyReset()
        {
            long secondsIntoInterval = Now() % GameConstants.DailyResetIntervalSeconds;
            return TimeSpan.FromSeconds(GameConstants.DailyResetIntervalSeconds - secondsIntoInterval);
        }

        public TimeSpan GetTimeUntilWeeklyReset()
        {
            long secondsIntoInterval = Now() % GameConstants.WeeklyResetIntervalSeconds;
            return TimeSpan.FromSeconds(GameConstants.WeeklyResetIntervalSeconds - secondsIntoInterval);
        }

        private static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
