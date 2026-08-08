using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Blocking foreground notice for emergencies/promos, distinct from NewsUI's browsable carousel.
    public class PopUpNoticeUI : UIController
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button dismissButton;
        [SerializeField] private Button linkButton;

        private NewsAnnouncementData boundAnnouncement;

        protected override void OnShown()
        {
            if (dismissButton != null) dismissButton.onClick.AddListener(Hide);
            if (linkButton != null) linkButton.onClick.AddListener(HandleLinkPressed);
        }

        protected override void OnHidden()
        {
            if (dismissButton != null) dismissButton.onClick.RemoveListener(Hide);
            if (linkButton != null) linkButton.onClick.RemoveListener(HandleLinkPressed);
        }

        public void ShowNotice(NewsAnnouncementData announcement)
        {
            boundAnnouncement = announcement;
            if (announcement == null) return;

            if (titleText != null) titleText.text = announcement.title;
            if (bodyText != null) bodyText.text = announcement.body;
            if (linkButton != null) linkButton.gameObject.SetActive(!string.IsNullOrEmpty(announcement.externalLink));

            Show();
        }

        private void HandleLinkPressed()
        {
            if (boundAnnouncement != null && !string.IsNullOrEmpty(boundAnnouncement.externalLink))
                Application.OpenURL(boundAnnouncement.externalLink);
        }
    }
}
