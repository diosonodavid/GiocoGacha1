using System;

namespace GachaGame.Core
{
    // One resting slot inside a dormitory room. Assigns a resting character by instance id and
    // grows a local affection counter the longer they occupy the slot; feeding that gain into the
    // account-wide AffinityManager is left to the caller, since this slot only owns its own state.
    public class DormitoryCharacterSlot
    {
        public string slotId;
        public string characterInstanceId;
        public long restStartUnix;
        public int affectionLevel;

        public bool IsOccupied => !string.IsNullOrEmpty(characterInstanceId);

        public void AssignCharacter(string instanceId)
        {
            characterInstanceId = instanceId;
            restStartUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void ClearSlot()
        {
            characterInstanceId = null;
            restStartUnix = 0;
        }

        // Grants affectionLevel points for each full hour rested since the last call; carries any
        // partial-hour remainder forward instead of discarding it.
        public int ApplyRestAffectionGain(int pointsPerHour)
        {
            if (!IsOccupied || pointsPerHour <= 0) return 0;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsedSeconds = Math.Max(0, now - restStartUnix);
            long hoursRested = elapsedSeconds / 3600;
            if (hoursRested <= 0) return 0;

            int gained = (int)hoursRested * pointsPerHour;
            affectionLevel += gained;
            restStartUnix = now - elapsedSeconds % 3600;
            return gained;
        }
    }
}
