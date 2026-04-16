using UnityEngine;

namespace JamTemplate.Audio
{
    public enum AudioBackendType
    {
        UnityAudio,
        Wwise
    }

    /// <summary>
    /// Configuration asset for the audio system.
    /// Create via Assets > Create > JamTemplate > Audio Config.
    /// Controls which audio backend is active at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "JamTemplate/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Tooltip("Which audio backend to use. Wwise requires the WWISE_ENABLED scripting define.")]
        public AudioBackendType backendType = AudioBackendType.UnityAudio;

        [Header("Music")]
        [Tooltip("Default crossfade duration in seconds when switching tracks.")]
        public float defaultMusicFadeTime = 1f;

        [Header("SFX")]
        [Tooltip("Initial size of the SFX AudioSource pool.")]
        public int sfxPoolSize = 8;
    }
}
