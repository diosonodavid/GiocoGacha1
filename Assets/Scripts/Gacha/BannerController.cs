using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Gacha
{
    // Picks the currently-live banner out of a configured list by comparing each BannerData's
    // start/end date strings against the current time, and groups the banner's pullable
    // characters by rarity for GachaManager/GachaSystem to draw from.
    public class BannerController : MonoBehaviour
    {
        private const string DateFormat = "yyyy-MM-dd";

        [SerializeField] private List<BannerData> banners = new();
        [SerializeField] private List<CharacterBaseData> characterPool = new();

        private Dictionary<Rarity, List<CharacterBaseData>> groupedPool;

        public IReadOnlyList<BannerData> Banners => banners;
        public BannerData ActiveBanner { get; private set; }

        public IReadOnlyDictionary<Rarity, List<CharacterBaseData>> CharacterPool
        {
            get
            {
                groupedPool ??= BuildCharacterPool();
                return groupedPool;
            }
        }

        private void Awake()
        {
            groupedPool = BuildCharacterPool();
            RefreshActiveBanner();
        }

        public void RefreshActiveBanner()
        {
            var now = DateTime.UtcNow;
            ActiveBanner = banners.FirstOrDefault(banner => IsBannerActive(banner, now));
        }

        public bool SetActiveBanner(string bannerId)
        {
            var banner = banners.FirstOrDefault(b => b != null && b.bannerId == bannerId);
            if (banner == null) return false;

            ActiveBanner = banner;
            return true;
        }

        // A banner with an unparsable/empty start or end date is treated as open-ended on that
        // side, so banners can be configured "permanent" simply by leaving the date fields blank.
        private static bool IsBannerActive(BannerData banner, DateTime now)
        {
            if (banner == null) return false;

            bool hasStarted = !TryParseDate(banner.startDate, out var start) || now >= start;
            bool hasNotEnded = !TryParseDate(banner.endDate, out var end) || now <= end;
            return hasStarted && hasNotEnded;
        }

        private static bool TryParseDate(string value, out DateTime result)
        {
            return DateTime.TryParseExact(
                value, DateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result);
        }

        private Dictionary<Rarity, List<CharacterBaseData>> BuildCharacterPool()
        {
            var pool = new Dictionary<Rarity, List<CharacterBaseData>>();
            foreach (var character in characterPool)
            {
                if (character == null) continue;

                if (!pool.TryGetValue(character.rarity, out var list))
                {
                    list = new List<CharacterBaseData>();
                    pool[character.rarity] = list;
                }
                list.Add(character);
            }
            return pool;
        }
    }
}
