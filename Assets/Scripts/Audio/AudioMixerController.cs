using UnityEngine;
using UnityEngine.Audio;

namespace GachaGame.Audio
{
    // Bridges linear 0-1 volume sliders to an AudioMixer's exposed decibel parameters (standard
    // Log10*20 conversion, since AudioMixer volume is logarithmic); an optional companion to
    // AudioManager for projects that route AudioSources through mixer groups instead of (or in
    // addition to) plain AudioSource.volume.
    public class AudioMixerController : MonoBehaviour
    {
        private const float MinDecibels = -80f;

        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterParameter = "MasterVolume";
        [SerializeField] private string bgmParameter = "BGMVolume";
        [SerializeField] private string sfxParameter = "SFXVolume";
        [SerializeField] private string voiceParameter = "VoiceVolume";

        public void SetMasterVolume(float linearVolume) => SetChannelVolume(masterParameter, linearVolume);
        public void SetBgmVolume(float linearVolume) => SetChannelVolume(bgmParameter, linearVolume);
        public void SetSfxVolume(float linearVolume) => SetChannelVolume(sfxParameter, linearVolume);
        public void SetVoiceVolume(float linearVolume) => SetChannelVolume(voiceParameter, linearVolume);

        private void SetChannelVolume(string parameterName, float linearVolume)
        {
            if (audioMixer == null || string.IsNullOrEmpty(parameterName)) return;

            float decibels = linearVolume <= 0.0001f ? MinDecibels : Mathf.Log10(linearVolume) * 20f;
            audioMixer.SetFloat(parameterName, decibels);
        }
    }
}
