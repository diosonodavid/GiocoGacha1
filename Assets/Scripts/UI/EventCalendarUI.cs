using System.Collections.Generic;
using GachaGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class EventCalendarUI : UIController
    {
        [SerializeField] private Transform activeEventListContainer;
        [SerializeField] private Transform upcomingEventListContainer;
        [SerializeField] private GameObject eventEntryPrefab;
        [SerializeField] private Text dailyResetText;
        [SerializeField] private Text weeklyResetText;

        private EventCalendarManager calendarManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out calendarManager);
            if (calendarManager != null) calendarManager.OnCalendarUpdated += RefreshCalendar;
            RefreshCalendar();
        }

        protected override void OnHidden()
        {
            if (calendarManager != null) calendarManager.OnCalendarUpdated -= RefreshCalendar;
        }

        private void Update()
        {
            if (!IsShown || calendarManager == null) return;

            if (dailyResetText != null) dailyResetText.text = calendarManager.GetTimeUntilDailyReset().ToString(@"hh\:mm\:ss");
            if (weeklyResetText != null) weeklyResetText.text = calendarManager.GetTimeUntilWeeklyReset().ToString(@"dd\.hh\:mm\:ss");
        }

        private void RefreshCalendar()
        {
            if (calendarManager == null) return;

            RebuildList(activeEventListContainer, calendarManager.GetActiveEvents());
            RebuildList(upcomingEventListContainer, calendarManager.GetUpcomingEvents());
        }

        private void RebuildList(Transform container, IEnumerable<CalendarEvent> events)
        {
            if (container == null) return;

            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);

            foreach (var calendarEvent in events)
            {
                if (eventEntryPrefab == null) continue;

                var go = Instantiate(eventEntryPrefab, container);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = calendarEvent.eventName;
            }
        }
    }
}
