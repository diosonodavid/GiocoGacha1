using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class NewsUI : UIController
    {
        [SerializeField] private Transform carouselContainer;
        [SerializeField] private GameObject bannerEntryPrefab;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;

        private NewsManager newsManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out newsManager);
            if (newsManager != null) newsManager.OnAnnouncementsUpdated += RebuildCarousel;
            RebuildCarousel();
        }

        protected override void OnHidden()
        {
            if (newsManager != null) newsManager.OnAnnouncementsUpdated -= RebuildCarousel;
        }

        private void RebuildCarousel()
        {
            if (carouselContainer == null || newsManager == null) return;

            for (int i = carouselContainer.childCount - 1; i >= 0; i--)
                Destroy(carouselContainer.GetChild(i).gameObject);

            NewsAnnouncementData firstAnnouncement = null;

            foreach (var announcement in newsManager.ActiveAnnouncements)
            {
                firstAnnouncement ??= announcement;
                if (bannerEntryPrefab == null) continue;

                var entry = Instantiate(bannerEntryPrefab, carouselContainer);
                var button = entry.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => ShowAnnouncement(announcement));
            }

            if (firstAnnouncement != null) ShowAnnouncement(firstAnnouncement);
        }

        private void ShowAnnouncement(NewsAnnouncementData announcement)
        {
            if (announcement == null) return;
            if (titleText != null) titleText.text = announcement.title;
            if (bodyText != null) bodyText.text = announcement.body;
        }
    }
}
