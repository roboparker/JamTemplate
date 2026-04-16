using UnityEngine;

namespace JamTemplate.Audio
{
    /// <summary>
    /// Abstraction for audio playback.
    /// Decouples game code from the concrete audio implementation (Unity Audio vs Wwise).
    /// All game code calls AudioManager → IAudioBackend, never the backend directly.
    /// </summary>
    public interface IAudioBackend
    {
        /// <summary>Play a music track, optionally crossfading from the current one.</summary>
        void PlayMusic(AudioClip clip, float fadeTime = 1f);

        /// <summary>Stop the current music track with an optional fade out.</summary>
        void StopMusic(float fadeTime = 1f);

        /// <summary>Play a one-shot sound effect.</summary>
        void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f);

        /// <summary>Set the master volume (controls AudioListener.volume).</summary>
        void SetMasterVolume(float volume);

        /// <summary>Set the music volume (0-1).</summary>
        void SetMusicVolume(float volume);

        /// <summary>Set the SFX volume (0-1).</summary>
        void SetSFXVolume(float volume);

        float MasterVolume { get; }
        float MusicVolume { get; }
        float SFXVolume { get; }

        /// <summary>Pause all audio (e.g. when game is paused).</summary>
        void PauseAudio();

        /// <summary>Resume all audio.</summary>
        void ResumeAudio();

        /// <summary>Initialize the backend. Called by AudioManager on Awake.</summary>
        void Initialize(GameObject owner);

        /// <summary>Clean up resources.</summary>
        void Dispose();
    }
}
