using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "BannerData", menuName = "GachaGame/Data/Banner Data")]
    public class BannerData : ScriptableObject
    {
        public string bannerId;
        public string bannerName;
        public string featuredCharacterId;
        public string startDate;
        public string endDate;
        public int costPerPull;
    }
}
