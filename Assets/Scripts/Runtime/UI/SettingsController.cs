using UnityEngine;
using UnityEngine.UI;
using JamTemplate.Audio;
using JamTemplate.Scene;

namespace JamTemplate.UI
{
    /// <summary>
    /// Controls the Settings additive scene.
    /// Master, Music, and SFX volume sliders.
    /// Persists values via AudioManager (which delegates to SaveManager).
    /// Works from both Title and Pause contexts.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [Header("Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Buttons")]
        [SerializeField] private Button backButton;

        private void Start()
        {
            LoadSettings();

            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        private void LoadSettings()
        {
            if (AudioManager.Instance != null)
            {
                if (masterVolumeSlider != null)
                    masterVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MasterVolume);
                if (musicVolumeSlider != null)
                    musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolumeProp);
                if (sfxVolumeSlider != null)
                    sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolumeProp);
            }
            else
            {
                // Fallback: default to 1.0
                if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(1f);
                if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(1f);
                if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(1f);
            }
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMasterVolume(value);
            else
                AudioListener.volume = value;
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMusicVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetSFXVolume(value);
        }

        public void OnBackClicked()
        {
            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.Back();
        }
    }
}
