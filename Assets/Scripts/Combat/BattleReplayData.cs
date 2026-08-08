using System;
using System.Collections.Generic;

namespace GachaGame.Combat
{
    [Serializable]
    public class BattleReplayCommand
    {
        public int turnIndex;
        public string casterId;
        public string skillId;
        public List<string> targetIds = new();
        public float timestampSeconds;
    }

    // Enough to deterministically re-simulate a battle: the RNG seed it started with plus the
    // ordered command stream issued by each side. Playback re-runs the same skill executions rather
    // than storing frame-by-frame state.
    [Serializable]
    public class BattleReplayData
    {
        public string replayId;
        public string battleId;
        public int randomSeed;
        public long recordedAtUnix;
        public List<BattleReplayCommand> commands = new();
    }
}
