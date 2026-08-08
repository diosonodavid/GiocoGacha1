using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Core
{
    public class PlacedFurniture
    {
        public FurnitureData data;
        public Vector2 roomPosition;
    }

    // Owns the room's furniture layout and derives passive gold/stamina generation from total
    // comfort, using the same elapsed-interval convention as CurrencyManager.ApplyPassiveStaminaRegen
    // so progress survives the app being closed.
    public class HousingManager : MonoBehaviour, IService
    {
        [SerializeField] private int generationIntervalSeconds = 1800; // 30 minutes
        [SerializeField] private int goldPerComfortPerInterval = 2;
        [SerializeField] private int comfortPerStaminaPoint = 50;

        public event Action<FurnitureData> OnFurniturePlaced;

        private readonly List<PlacedFurniture> placedFurniture = new();
        private CurrencyManager currencyManager;

        public IReadOnlyList<PlacedFurniture> PlacedFurniture => placedFurniture;
        public int TotalComfort => placedFurniture.Sum(f => f.data != null ? f.data.comfortPoints : 0);

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out currencyManager);
            Debug.Log($"{nameof(HousingManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void PlaceFurniture(FurnitureData data, Vector2 position)
        {
            if (data == null) return;
            placedFurniture.Add(new PlacedFurniture { data = data, roomPosition = position });
            OnFurniturePlaced?.Invoke(data);
        }

        public void RemoveFurniture(FurnitureData data) => placedFurniture.RemoveAll(f => f.data == data);

        // Call periodically (e.g. on app resume) with the save's last-generation timestamp; grants
        // whole intervals elapsed and advances the timestamp only by the intervals consumed.
        public void ApplyPassiveGeneration(ref long lastGenerationUnix)
        {
            if (currencyManager == null) return;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsedSeconds = Math.Max(0, now - lastGenerationUnix);
            int intervalsElapsed = (int)(elapsedSeconds / generationIntervalSeconds);
            if (intervalsElapsed <= 0) return;

            int comfort = TotalComfort;
            int goldGained = comfort * goldPerComfortPerInterval * intervalsElapsed;
            int staminaGained = comfortPerStaminaPoint > 0 ? (comfort / comfortPerStaminaPoint) * intervalsElapsed : 0;

            if (goldGained > 0) currencyManager.AddCurrency(CurrencyType.Gold, goldGained);
            if (staminaGained > 0) currencyManager.AddCurrency(CurrencyType.Stamina, staminaGained);

            lastGenerationUnix += intervalsElapsed * (long)generationIntervalSeconds;
        }
    }
}
