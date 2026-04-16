using UnityEngine;
using UnityEngine.UI;
using JamTemplate.Save;
using JamTemplate.Scene;

namespace JamTemplate.UI
{
    /// <summary>
    /// Controls the Settings additive scene.
    /// Master, Music, and SFX volume sliders.
    /// Persists values via SaveManager.
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

        // Save keys
        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string SfxVolumeKey = "SfxVolume";

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
            if (SaveManager.Instance == null) return;

            float master = SaveManager.Instance.LoadFloat(MasterVolumeKey, 1f);
            float music = SaveManager.Instance.LoadFloat(MusicVolumeKey, 1f);
            float sfx = SaveManager.Instance.LoadFloat(SfxVolumeKey, 1f);

            if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(master);
            if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(music);
            if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(sfx);
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Save(MasterVolumeKey, value);

            // TODO: Wire to AudioManager when audio system is implemented
            AudioListener.volume = value;
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Save(MusicVolumeKey, value);

            // TODO: Wire to AudioManager.SetMusicVolume() when audio system is implemented
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Save(SfxVolumeKey, value);

            // TODO: Wire to AudioManager.SetSfxVolume() when audio system is implemented
        }

        public void OnBackClicked()
        {
            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.Back();
        }
    }
}
