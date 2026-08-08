using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class VoiceLineEntry
    {
        public string eventId; // e.g. "battle_start", "skill_ultimate", "victory"
        public AudioClip clip;
        [TextArea] public string subtitle;
    }

    [CreateAssetMenu(fileName = "VoiceLineData", menuName = "GachaGame/Data/Voice Line Data")]
    public class VoiceLineData : ScriptableObject
    {
        public string characterBaseDataId;
        public List<VoiceLineEntry> lines = new();
    }
}
