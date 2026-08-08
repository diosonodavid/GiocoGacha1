using System;
using System.Collections.Generic;
using GachaGame.Data;

namespace GachaGame.Core
{
    // Rolling log of the last MaxEntries summons, oldest first.
    public class GachaHistory
    {
        public const int MaxEntries = 100;

        private readonly List<GachaHistoryEntry> entries = new();

        public IReadOnlyList<GachaHistoryEntry> Entries => entries;

        public void Record(GachaPullResult result)
        {
            if (result.pulledCharacter == null) return;

            entries.Add(new GachaHistoryEntry
            {
                characterId = result.pulledCharacter.id,
                characterName = result.pulledCharacter.characterName,
                rarity = result.rarity,
                isNew = result.isNew,
                timestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            if (entries.Count > MaxEntries)
                entries.RemoveAt(0);
        }
    }
}
