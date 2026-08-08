using System;

namespace GachaGame.Data
{
    // Plain serializable record (not a ScriptableObject) since announcements come from the server
    // at runtime rather than being authored as project assets.
    [Serializable]
    public class NewsAnnouncementData
    {
        public string announcementId;
        public string title;
        public string body;
        public string bannerImageUrl;
        public string externalLink;
        public long startTimeUnix;
        public long endTimeUnix;
        public bool isBlockingPopup;
    }
}
