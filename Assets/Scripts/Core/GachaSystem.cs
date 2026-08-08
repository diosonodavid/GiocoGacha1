using System.Collections.Generic;
using System.Linq;
using GachaGame.Data;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Core
{
    // Probability tables: 5* 1.5%, 4* 8.5%, 3* 90.0%, with soft pity ramping the 5* rate from
    // pull 70 and a hard guarantee at pull 90 (GameConstants.SoftPityThreshold/HardPityThreshold).
    public class GachaSystem
    {
        private const int PullsPerTenPull = 10;

        private readonly System.Random random;

        public GachaSystem(System.Random random = null)
        {
            this.random = random ?? new System.Random();
        }

        public static float GetFiveStarRate(int pullNumber)
        {
            if (pullNumber >= GameConstants.HardPityThreshold) return 100f;

            if (pullNumber >= GameConstants.SoftPityThreshold)
            {
                int stepsIntoSoftPity = pullNumber - GameConstants.SoftPityThreshold + 1;
                return Mathf.Min(100f, GameConstants.GachaFiveStarBaseRate + stepsIntoSoftPity * GameConstants.GachaSoftPityRateIncreasePerPull);
            }

            return GameConstants.GachaFiveStarBaseRate;
        }

        public GachaPullResult PerformPull(
            GachaPityState pity,
            IReadOnlyDictionary<Rarity, List<CharacterBaseData>> pool,
            string featuredCharacterId,
            IReadOnlyCollection<string> ownedCharacterIds)
        {
            int pullNumber = pity.pullsSinceLastFiveStar + 1;
            Rarity rarity = RollRarity(pullNumber);
            return ResolvePull(pity, rarity, pool, featuredCharacterId, ownedCharacterIds);
        }

        // Ten-pull banner ritual: identical to ten independent pulls, except the last slot is
        // upgraded to a guaranteed 4* (or better) if none has appeared yet in the batch.
        public List<GachaPullResult> PerformTenPull(
            GachaPityState pity,
            IReadOnlyDictionary<Rarity, List<CharacterBaseData>> pool,
            string featuredCharacterId,
            IReadOnlyCollection<string> ownedCharacterIds)
        {
            var results = new List<GachaPullResult>(PullsPerTenPull);
            bool hasFourStarOrHigher = false;

            for (int i = 0; i < PullsPerTenPull; i++)
            {
                bool isLastPull = i == PullsPerTenPull - 1;
                int pullNumber = pity.pullsSinceLastFiveStar + 1;

                Rarity rarity = RollRarity(pullNumber);
                if (isLastPull && !hasFourStarOrHigher && rarity == Rarity.ThreeStars)
                    rarity = Rarity.FourStars;

                var result = ResolvePull(pity, rarity, pool, featuredCharacterId, ownedCharacterIds);
                if (result.rarity is Rarity.FourStars or Rarity.FiveStars)
                    hasFourStarOrHigher = true;

                results.Add(result);
            }

            return results;
        }

        private Rarity RollRarity(int pullNumber)
        {
            float fiveStarRate = GetFiveStarRate(pullNumber);
            float fourStarRate = Mathf.Max(0f, Mathf.Min(GameConstants.GachaFourStarBaseRate, 100f - fiveStarRate));

            double roll = random.NextDouble() * 100.0;
            if (roll < fiveStarRate) return Rarity.FiveStars;
            if (roll < fiveStarRate + fourStarRate) return Rarity.FourStars;
            return Rarity.ThreeStars;
        }

        private GachaPullResult ResolvePull(
            GachaPityState pity,
            Rarity rarity,
            IReadOnlyDictionary<Rarity, List<CharacterBaseData>> pool,
            string featuredCharacterId,
            IReadOnlyCollection<string> ownedCharacterIds)
        {
            pity.pullsSinceLastFiveStar = rarity == Rarity.FiveStars ? 0 : pity.pullsSinceLastFiveStar + 1;

            CharacterBaseData pulled = rarity == Rarity.FiveStars && pool.TryGetValue(Rarity.FiveStars, out var fiveStarPool)
                ? SelectFiveStarCharacter(pity, fiveStarPool, featuredCharacterId)
                : SelectRandomFromPool(pool, rarity);

            bool isNew = pulled != null && (ownedCharacterIds == null || !ownedCharacterIds.Contains(pulled.id));

            return new GachaPullResult
            {
                pulledCharacter = pulled,
                rarity = rarity,
                isNew = isNew,
                convertedMemoryFragment = pulled != null && !isNew
            };
        }

        // 50/50: a loss guarantees the featured character on the next 5* pull.
        private CharacterBaseData SelectFiveStarCharacter(GachaPityState pity, List<CharacterBaseData> fiveStarPool, string featuredCharacterId)
        {
            var featured = fiveStarPool.Find(c => c.id == featuredCharacterId);
            if (featured == null) return fiveStarPool[random.Next(fiveStarPool.Count)];

            bool wins5050 = pity.isFiftyFiftyGuaranteed || random.NextDouble() < 0.5;
            pity.isFiftyFiftyGuaranteed = !wins5050;

            if (wins5050) return featured;

            var offBannerPool = fiveStarPool.FindAll(c => c.id != featuredCharacterId);
            return offBannerPool.Count > 0 ? offBannerPool[random.Next(offBannerPool.Count)] : featured;
        }

        private CharacterBaseData SelectRandomFromPool(IReadOnlyDictionary<Rarity, List<CharacterBaseData>> pool, Rarity rarity)
        {
            if (!pool.TryGetValue(rarity, out var candidates) || candidates.Count == 0) return null;
            return candidates[random.Next(candidates.Count)];
        }
    }
}
