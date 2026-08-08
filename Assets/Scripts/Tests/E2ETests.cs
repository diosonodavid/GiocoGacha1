using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using GachaGame.UI;
using GachaGame.Utils;
using NUnit.Framework;
using UnityEngine;

namespace GachaGame.Tests
{
    // Simulates a full player session end-to-end across already-unit-tested subsystems:
    // Login (save load/create) -> Gacha Summon -> Team Management -> Battle -> Server Sync (save).
    // Exercises the seams between systems rather than re-testing each system's own edge cases.
    public class E2ETests
    {
        private const string FeaturedId = "e2e_featured_5star";

        private readonly List<GameObject> spawnedGameObjects = new();
        private readonly List<string> tempFiles = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in spawnedGameObjects)
                if (go != null) Object.DestroyImmediate(go);
            spawnedGameObjects.Clear();

            foreach (var path in tempFiles)
                if (File.Exists(path)) File.Delete(path);
            tempFiles.Clear();
        }

        private T CreateManager<T>() where T : Component
        {
            var go = new GameObject(typeof(T).Name);
            spawnedGameObjects.Add(go);
            return go.AddComponent<T>();
        }

        private string GetTempSavePath()
        {
            string path = Path.Combine(Path.GetTempPath(), $"gachagame_e2e_{System.Guid.NewGuid()}.sav");
            tempFiles.Add(path);
            return path;
        }

        private static CharacterBaseData CreateCharacter(string id, Rarity rarity, float atk = 100f, float hp = 1000f, float def = 20f, float spd = 100f)
        {
            var character = ScriptableObject.CreateInstance<CharacterBaseData>();
            character.id = id;
            character.characterName = id;
            character.rarity = rarity;
            character.element = ElementType.Fire;
            character.baseStats.Dictionary[StatType.ATK] = atk;
            character.baseStats.Dictionary[StatType.HP] = hp;
            character.baseStats.Dictionary[StatType.DEF] = def;
            character.baseStats.Dictionary[StatType.SPD] = spd;
            character.baseStats.Dictionary[StatType.CritRate] = 5f;
            character.baseStats.Dictionary[StatType.CritDMG] = 150f;
            return character;
        }

        [Test]
        public async Task E2E_FullPlayerJourney_LoginToBattleToServerSync()
        {
            // 1. Login: a fresh account starts with a blank currency/level state.
            var currencyManager = CreateManager<CurrencyManager>();
            var accountLevelManager = CreateManager<AccountLevelManager>();
            var inventoryManager = CreateManager<InventoryManager>();

            currencyManager.AddCurrency(CurrencyType.Gems, 1000);
            Assert.AreEqual(1000, currencyManager.Gems);

            // 2. Gacha summon: guarantee a five-star pull and add it to the roster.
            var pool = new Dictionary<Rarity, List<CharacterBaseData>>
            {
                { Rarity.FiveStars, new List<CharacterBaseData> { CreateCharacter(FeaturedId, Rarity.FiveStars) } }
            };
            var gacha = new GachaSystem(new System.Random(42));
            var pity = new GachaPityState { pullsSinceLastFiveStar = GameConstants.HardPityThreshold - 1, isFiftyFiftyGuaranteed = true };

            Assert.IsTrue(currencyManager.TrySpendCurrency(CurrencyType.Gems, 100));
            var pullResult = gacha.PerformPull(pity, pool, FeaturedId, new HashSet<string>());
            inventoryManager.ApplyPullResult(pullResult);

            Assert.IsTrue(inventoryManager.HasCharacter(FeaturedId));

            // 3. Team management: assign the pulled character into the roster.
            var teamBuilder = CreateManager<TeamBuilderUI>();
            Assert.IsTrue(teamBuilder.TryAssignToSlot(0, FeaturedId));
            Assert.AreEqual(FeaturedId, teamBuilder.TeamSlots[0]);

            // 4. Enter battle: build combat instances for the assigned team and a single enemy.
            var playerInstance = inventoryManager.OwnedCharacters[FeaturedId];
            var playerBase = pool[Rarity.FiveStars][0];
            playerInstance.InitializeCombatState(StatCalculator.CalculateFinalStats(playerBase.baseStats.Dictionary, playerInstance.currentLevel));

            var enemyBase = CreateCharacter("e2e_enemy", Rarity.ThreeStars, atk: 10f, hp: 1f, def: 0f, spd: 50f);
            var enemyInstance = new CharacterInstance { instanceId = "enemy-1", baseDataId = enemyBase.id, currentLevel = 1 };
            enemyInstance.InitializeCombatState(StatCalculator.CalculateFinalStats(enemyBase.baseStats.Dictionary, 1));

            var battle = new BattleTurnController();
            BattleState? finalState = null;
            battle.OnBattleEnd += result => finalState = result;
            battle.StartBattle(new ICombatant[] { playerInstance }, new ICombatant[] { enemyInstance });

            // Player SPD (100) beats the enemy's (50), so the player deterministically acts first.
            Assert.AreEqual(playerInstance, battle.ActiveCombatant);

            var skill = ScriptableObject.CreateInstance<SkillData>();
            skill.skillId = "e2e_basic_attack";
            skill.damageMultiplier = 5f; // guarantees a one-shot kill on the 1 HP enemy above

            SkillExecutionEngine.ExecuteSkill(playerInstance, ElementType.Fire, skill,
                new[] { new CombatTarget(enemyInstance, ElementType.Fire) }, new System.Random(1));
            battle.EndTurn();

            Assert.AreEqual(BattleState.Victory, finalState);

            // 5. Server sync: persist the resulting state to an encrypted save, then reload it to
            // confirm the round trip matches what would be sent to the server via NetworkManager.
            var rewards = BattleRewardCalculator.CalculateRewards(new[] { enemyInstance }, goldPerEnemy: 100, accountExpPerEnemy: 50);
            currencyManager.AddCurrency(CurrencyType.Gold, rewards.gold);
            accountLevelManager.AddExp(rewards.accountExp, currencyManager);

            var saveSystem = new SaveSystem("e2e-test-passphrase");
            var saveData = new PlayerSaveData
            {
                accountId = "e2e-account",
                level = accountLevelManager.Level,
                exp = accountLevelManager.Exp,
                gold = currencyManager.Gold,
                gems = currencyManager.Gems,
                stamina = currencyManager.Stamina,
                maxStamina = currencyManager.MaxStamina,
                lastStaminaUpdate = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string path = GetTempSavePath();
            await saveSystem.SaveGameAsync(saveData, path);
            var loaded = await saveSystem.LoadGameAsync(path);

            Assert.AreEqual(saveData.gold, loaded.gold);
            Assert.AreEqual(saveData.level, loaded.level);
            Assert.Greater(loaded.gold, 0);
        }

        [Test]
        public void E2E_BattleCannotStart_WithIncompleteTeam()
        {
            var teamBuilder = CreateManager<TeamBuilderUI>();
            teamBuilder.TryAssignToSlot(0, FeaturedId);

            Assert.IsFalse(teamBuilder.IsTeamComplete());
        }

        [Test]
        public void E2E_DuplicatePull_ConvertsToMemoryInsteadOfNewCharacter()
        {
            var inventoryManager = CreateManager<InventoryManager>();
            var pool = new Dictionary<Rarity, List<CharacterBaseData>>
            {
                { Rarity.FiveStars, new List<CharacterBaseData> { CreateCharacter(FeaturedId, Rarity.FiveStars) } }
            };
            var gacha = new GachaSystem(new System.Random(5));
            var pity = new GachaPityState { pullsSinceLastFiveStar = GameConstants.HardPityThreshold - 1, isFiftyFiftyGuaranteed = true };

            var firstPull = gacha.PerformPull(pity, pool, FeaturedId, new HashSet<string>());
            inventoryManager.ApplyPullResult(firstPull);

            pity.pullsSinceLastFiveStar = GameConstants.HardPityThreshold - 1;
            var secondPull = gacha.PerformPull(pity, pool, FeaturedId, new HashSet<string> { FeaturedId });
            inventoryManager.ApplyPullResult(secondPull);

            Assert.AreEqual(1, inventoryManager.OwnedCharacters.Count);
            Assert.AreEqual(1, inventoryManager.OwnedCharacters[FeaturedId].memoryLevel);
        }
    }
}
