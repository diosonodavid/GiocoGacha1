using System;
using GachaGame.Data;

namespace GachaGame.Combat
{
    // Resolves which WorldBossData phase should be active for a given remaining-HP fraction and
    // fires an event when it advances (new attack pattern / visual change). Deliberately decoupled
    // from WorldBossManager - like DormitoryCharacterSlot/AffinityManager elsewhere in this codebase
    // - so feeding damage updates into it is left to the caller instead of this class reaching into
    // WorldBossManager directly.
    public class BossPhaseController
    {
        public event Action<WorldBossPhase> OnPhaseChanged;

        private readonly WorldBossData bossData;

        public int CurrentPhaseIndex { get; private set; } = -1;
        public WorldBossPhase CurrentPhase =>
            bossData != null && CurrentPhaseIndex >= 0 && CurrentPhaseIndex < bossData.phaseDataList.Count
                ? bossData.phaseDataList[CurrentPhaseIndex]
                : null;

        public BossPhaseController(WorldBossData data)
        {
            bossData = data;
        }

        // Call with RemainingHp / totalHP whenever the boss takes damage; advances to the highest
        // phase whose threshold has been crossed, never regresses to an earlier phase.
        public void EvaluateHpPercent(float remainingHpPercent)
        {
            if (bossData == null) return;

            for (int i = bossData.phaseDataList.Count - 1; i > CurrentPhaseIndex; i--)
            {
                if (remainingHpPercent > bossData.phaseDataList[i].hpThresholdPercent) continue;

                CurrentPhaseIndex = i;
                OnPhaseChanged?.Invoke(bossData.phaseDataList[i]);
                break;
            }
        }
    }
}
