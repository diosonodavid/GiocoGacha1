using GachaGame.Audio;
using GachaGame.Core;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Separate volume sliders for Master/BGM/SFX/Voice; drives AudioManager (the source of truth
    // for per-channel volume) and, if a mixer is used in this scene, mirrors the same values onto
    // AudioMixerController so both stay in sync.
    public class AudioSettingsUI : UIController
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider voiceSlider;
        [SerializeField] private AudioMixerController mixerController;

        private AudioManager audioManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out audioManager);
            if (audioManager == null) return;

            InitializeSlider(masterSlider, 1f, OnMasterChanged);
            InitializeSlider(bgmSlider, audioManager.BgmVolume, OnBgmChanged);
            InitializeSlider(sfxSlider, audioManager.SfxVolume, OnSfxChanged);
            InitializeSlider(voiceSlider, audioManager.VoiceVolume, OnVoiceChanged);
        }

        private void InitializeSlider(Slider slider, float value, UnityAction<float> onChanged)
        {
            if (slider == null) return;
            slider.onValueChanged.RemoveListener(onChanged);
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(onChanged);
        }

        private void OnMasterChanged(float value) => mixerController?.SetMasterVolume(value);

        private void OnBgmChanged(float value)
        {
            audioManager.SetBgmVolume(value);
            mixerController?.SetBgmVolume(value);
        }

        private void OnSfxChanged(float value)
        {
            audioManager.SetSfxVolume(value);
            mixerController?.SetSfxVolume(value);
        }

        private void OnVoiceChanged(float value)
        {
            audioManager.SetVoiceVolume(value);
            mixerController?.SetVoiceVolume(value);
        }
    }
}
