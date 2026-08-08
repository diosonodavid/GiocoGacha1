using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Utils;
using NUnit.Framework;
using UnityEngine;

namespace GachaGame.Tests
{
    public class EconomyTests
    {
        private const string FeaturedId = "featured_5star";
        private const string OffBannerId = "offbanner_5star";
        private const string FourStarId = "standard_4star";
        private const string ThreeStarId = "standard_3star";

        private static CharacterBaseData CreateCharacter(string id, Rarity rarity)
        {
            var character = ScriptableObject.CreateInstance<CharacterBaseData>();
            character.id = id;
            character.characterName = id;
            character.rarity = rarity;
            return character;
        }

        private static Dictionary<Rarity, List<CharacterBaseData>> BuildStandardPool()
        {
            return new Dictionary<Rarity, List<CharacterBaseData>>
            {
                { Rarity.FiveStars, new List<CharacterBaseData> { CreateCharacter(FeaturedId, Rarity.FiveStars), CreateCharacter(OffBannerId, Rarity.FiveStars) } },
                { Rarity.FourStars, new List<CharacterBaseData> { CreateCharacter(FourStarId, Rarity.FourStars) } },
                { Rarity.ThreeStars, new List<CharacterBaseData> { CreateCharacter(ThreeStarId, Rarity.ThreeStars) } }
            };
        }

        private static string GetTempSavePath() => Path.Combine(Path.GetTempPath(), $"gachagame_test_{System.Guid.NewGuid()}.sav");

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        [Test]
        public void GachaSystem_FiveStarRate_EqualsBaseRateBeforeSoftPity()
        {
            Assert.AreEqual(GameConstants.GachaFiveStarBaseRate, GachaSystem.GetFiveStarRate(1));
            Assert.AreEqual(GameConstants.GachaFiveStarBaseRate, GachaSystem.GetFiveStarRate(GameConstants.SoftPityThreshold - 1));
        }

        [Test]
        public void GachaSystem_FiveStarRate_IncreasesDuringSoftPity()
        {
            float rateAtSoftPityStart = GachaSystem.GetFiveStarRate(GameConstants.SoftPityThreshold);
            float rateLater = GachaSystem.GetFiveStarRate(GameConstants.SoftPityThreshold + 5);

            Assert.Greater(rateAtSoftPityStart, GameConstants.GachaFiveStarBaseRate);
            Assert.Greater(rateLater, rateAtSoftPityStart);
        }

        [Test]
        public void GachaSystem_FiveStarRate_Is100PercentAtHardPity()
        {
            Assert.AreEqual(100f, GachaSystem.GetFiveStarRate(GameConstants.HardPityThreshold));
        }

        [Test]
        public void GachaSystem_RarityDistribution_RoughlyMatchesConfiguredRates()
        {
            var pool = BuildStandardPool();
            var gacha = new GachaSystem(new System.Random(123));
            var pity = new GachaPityState();
            int fiveStarCount = 0, fourStarCount = 0, threeStarCount = 0;
            const int trials = 20000;

            for (int i = 0; i < trials; i++)
            {
                var result = gacha.PerformPull(pity, pool, FeaturedId, new HashSet<string>());
                switch (result.rarity)
                {
                    case Rarity.FiveStars: fiveStarCount++; break;
                    case Rarity.FourStars: fourStarCount++; break;
                    case Rarity.ThreeStars: threeStarCount++; break;
                }
            }

            float fiveStarPercent = fiveStarCount / (float)trials * 100f;
            float fourStarPercent = fourStarCount / (float)trials * 100f;

            Assert.AreEqual(trials, fiveStarCount + fourStarCount + threeStarCount);
            Assert.Greater(fiveStarPercent, 1.0f);
            Assert.Less(fiveStarPercent, 4.0f);
            Assert.Greater(fourStarPercent, 6.0f);
            Assert.Less(fourStarPercent, 11.0f);
        }

        [Test]
        public void GachaSystem_FiftyFiftyGuarantee_AlwaysAwardsFeaturedCharacter()
        {
            var pool = BuildStandardPool();
            var gacha = new GachaSystem(new System.Random(1));
            var pity = new GachaPityState
            {
                pullsSinceLastFiveStar = GameConstants.HardPityThreshold - 1, // forces a 5* this pull
                isFiftyFiftyGuaranteed = true
            };

            var result = gacha.PerformPull(pity, pool, FeaturedId, new HashSet<string>());

            Assert.AreEqual(Rarity.FiveStars, result.rarity);
            Assert.AreEqual(FeaturedId, result.pulledCharacter.id);
        }

        [Test]
        public void GachaSystem_FiftyFifty_NeverLosesTwiceInARow()
        {
            var pool = BuildStandardPool();
            var gacha = new GachaSystem(new System.Random(7));
            var pity = new GachaPityState();

            bool previousWasOffBanner = false;
            for (int i = 0; i < 500; i++)
            {
                pity.pullsSinceLastFiveStar = GameConstants.HardPityThreshold - 1; // force a 5* every pull
                var result = gacha.PerformPull(pity, pool, FeaturedId, new HashSet<string>());

                if (previousWasOffBanner)
                    Assert.AreEqual(FeaturedId, result.pulledCharacter.id, "A five-star loss must be followed by a guaranteed featured pull.");

                previousWasOffBanner = result.pulledCharacter.id == OffBannerId;
            }
        }

        [Test]
        public void GachaSystem_PerformTenPull_AlwaysIncludesFourStarOrHigher()
        {
            var pool = BuildStandardPool();
            var gacha = new GachaSystem(new System.Random(999));
            var pity = new GachaPityState();

            var results = gacha.PerformTenPull(pity, pool, FeaturedId, new HashSet<string>());

            Assert.AreEqual(10, results.Count);
            Assert.IsTrue(results.Exists(r => r.rarity is Rarity.FourStars or Rarity.FiveStars));
        }

        [Test]
        public void GachaSystem_PullingOwnedCharacter_IsNotMarkedNew()
        {
            var pool = BuildStandardPool();
            var gacha = new GachaSystem(new System.Random(1));
            var pity = new GachaPityState { pullsSinceLastFiveStar = GameConstants.HardPityThreshold - 1, isFiftyFiftyGuaranteed = true };

            var result = gacha.PerformPull(pity, pool, FeaturedId, new HashSet<string> { FeaturedId });

            Assert.IsFalse(result.isNew);
            Assert.IsTrue(result.convertedMemoryFragment);
        }

        [Test]
        public async Task SaveSystem_SaveThenLoad_RoundTripsData()
        {
            string filePath = GetTempSavePath();
            try
            {
                var saveSystem = new SaveSystem("test-passphrase");
                var data = new PlayerSaveData
                {
                    accountId = "acc1",
                    level = 5,
                    exp = 120,
                    gold = 1000,
                    gems = 50,
                    stamina = 80,
                    maxStamina = 120,
                    lastStaminaUpdate = 123456
                };

                await saveSystem.SaveGameAsync(data, filePath);
                var loaded = await saveSystem.LoadGameAsync(filePath);

                Assert.AreEqual(data.accountId, loaded.accountId);
                Assert.AreEqual(data.level, loaded.level);
                Assert.AreEqual(data.gold, loaded.gold);
                Assert.AreEqual(data.gems, loaded.gems);
                Assert.AreEqual(data.stamina, loaded.stamina);
            }
            finally
            {
                DeleteIfExists(filePath);
            }
        }

        [Test]
        public async Task SaveSystem_SavedFile_DoesNotContainPlainTextFields()
        {
            string filePath = GetTempSavePath();
            try
            {
                var saveSystem = new SaveSystem("test-passphrase");
                var data = new PlayerSaveData { accountId = "plaintext-marker-account", gold = 999999 };

                await saveSystem.SaveGameAsync(data, filePath);
                byte[] fileBytes = File.ReadAllBytes(filePath);
                string rawContent = Encoding.UTF8.GetString(fileBytes);

                Assert.IsFalse(rawContent.Contains("plaintext-marker-account"));
                Assert.IsFalse(rawContent.Contains("999999"));
            }
            finally
            {
                DeleteIfExists(filePath);
            }
        }

        [Test]
        public void SaveSystem_LoadGameAsync_ThrowsWhenHmacDoesNotMatchPayload()
        {
            string filePath = GetTempSavePath();
            try
            {
                var saveSystem = new SaveSystem("test-passphrase");
                string payload = JsonUtility.ToJson(new PlayerSaveData { accountId = "acc1", gold = 500 });
                string tamperedEnvelopeJson = JsonUtility.ToJson(new SaveEnvelope { payload = payload, hmac = "invalid-hmac-value" });

                byte[] encrypted = saveSystem.Encrypt(tamperedEnvelopeJson);
                File.WriteAllBytes(filePath, encrypted);

                Assert.ThrowsAsync<InvalidDataException>(async () => await saveSystem.LoadGameAsync(filePath));
            }
            finally
            {
                DeleteIfExists(filePath);
            }
        }

        [Test]
        public void SaveDataValidator_DetectsTamperedPayload()
        {
            byte[] key = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef");
            const string payload = "{\"gold\":100}";
            string hmac = SaveDataValidator.ComputeHmac(payload, key);

            Assert.IsTrue(SaveDataValidator.Validate(payload, hmac, key));
            Assert.IsFalse(SaveDataValidator.Validate("{\"gold\":999}", hmac, key));
        }
    }
}
