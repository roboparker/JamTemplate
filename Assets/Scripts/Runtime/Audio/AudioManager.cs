using UnityEngine;
using JamTemplate.Save;

namespace JamTemplate.Audio
{
    /// <summary>
    /// Singleton facade for the audio system.
    /// Delegates to the configured IAudioBackend.
    /// Persists volume settings via SaveManager.
    /// All game code calls AudioManager — never the backend directly.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioConfig config;

        private IAudioBackend backend;

        // Save keys
        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string SfxVolumeKey = "SfxVolume";

        public IAudioBackend Backend => backend;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBackend();
        }

        private void InitializeBackend()
        {
            var backendType = config != null ? config.backendType : AudioBackendType.UnityAudio;

            switch (backendType)
            {
#if WWISE_ENABLED
                case AudioBackendType.Wwise:
                    // WwiseAudioBackend will be implemented in Phase 6
                    Debug.LogWarning("[AudioManager] Wwise backend not yet implemented. Falling back to Unity Audio.");
                    CreateUnityAudioBackend();
                    break;
#endif
                case AudioBackendType.UnityAudio:
                default:
                    CreateUnityAudioBackend();
                    break;
            }

            // Restore saved volumes
            RestoreVolumes();
        }

        private void CreateUnityAudioBackend()
        {
            var unityBackend = new UnityAudioBackend();
            if (config != null)
                unityBackend.SetPoolSize(config.sfxPoolSize);
            unityBackend.Initialize(gameObject);
            backend = unityBackend;
        }

        private void RestoreVolumes()
        {
            if (SaveManager.Instance != null)
            {
                float master = SaveManager.Instance.LoadFloat(MasterVolumeKey, 1f);
                float music = SaveManager.Instance.LoadFloat(MusicVolumeKey, 1f);
                float sfx = SaveManager.Instance.LoadFloat(SfxVolumeKey, 1f);

                backend.SetMasterVolume(master);
                backend.SetMusicVolume(music);
                backend.SetSFXVolume(sfx);
            }
        }

        private void OnDestroy()
        {
            backend?.Dispose();
        }

        // ─────────────────────────────────────────────
        // PUBLIC API
        // ─────────────────────────────────────────────

        /// <summary>Play a music track with crossfade.</summary>
        public void PlayMusic(AudioClip clip, float fadeTime = -1f)
        {
            float fade = fadeTime >= 0 ? fadeTime : (config != null ? config.defaultMusicFadeTime : 1f);
            backend?.PlayMusic(clip, fade);
        }

        /// <summary>Stop the current music track.</summary>
        public void StopMusic(float fadeTime = -1f)
        {
            float fade = fadeTime >= 0 ? fadeTime : (config != null ? config.defaultMusicFadeTime : 1f);
            backend?.StopMusic(fade);
        }

        /// <summary>Play a one-shot sound effect.</summary>
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            backend?.PlaySFX(clip, volume, pitch);
        }

        /// <summary>Set master volume (0-1). Persists via SaveManager.</summary>
        public void SetMasterVolume(float volume)
        {
            backend?.SetMasterVolume(volume);
            if (SaveManager.Instance != null)
                SaveManager.Instance.Save(MasterVolumeKey, volume);
        }

        /// <summary>Set music volume (0-1). Persists via SaveManager.</summary>
        public void SetMusicVolume(float volume)
        {
            backend?.SetMusicVolume(volume);
            if (SaveManager.Instance != null)
                SaveManager.Instance.Save(MusicVolumeKey, volume);
        }

        /// <summary>Set SFX volume (0-1). Persists via SaveManager.</summary>
        public void SetSFXVolume(float volume)
        {
            backend?.SetSFXVolume(volume);
            if (SaveManager.Instance != null)
                SaveManager.Instance.Save(SfxVolumeKey, volume);
        }

        public float MasterVolume => backend?.MasterVolume ?? 1f;
        public float MusicVolumeProp => backend?.MusicVolume ?? 1f;
        public float SFXVolumeProp => backend?.SFXVolume ?? 1f;

        /// <summary>Pause all audio playback.</summary>
        public void PauseAudio()
        {
            backend?.PauseAudio();
        }

        /// <summary>Resume all audio playback.</summary>
        public void ResumeAudio()
        {
            backend?.ResumeAudio();
        }
    }
}
