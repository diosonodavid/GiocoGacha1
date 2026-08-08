using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Audio
{
    // Looks up a character's voice bank by event id and routes playback through AudioManager's
    // dedicated voice channel, so voice lines respect the player's voice volume slider like any
    // other audio.
    public class VoiceController : MonoBehaviour
    {
        [SerializeField] private List<VoiceLineData> voiceBanks = new();

        private AudioManager audioManager;

        private void Awake() => ServiceLocator.Instance.TryGet(out audioManager);

        public void PlayLine(string characterBaseDataId, string eventId)
        {
            if (audioManager == null || string.IsNullOrEmpty(characterBaseDataId)) return;

            foreach (var bank in voiceBanks)
            {
                if (bank == null || bank.characterBaseDataId != characterBaseDataId) continue;

                foreach (var line in bank.lines)
                {
                    if (line.eventId != eventId) continue;
                    audioManager.PlayVoice(line.clip);
                    return;
                }
            }
        }
    }
}
