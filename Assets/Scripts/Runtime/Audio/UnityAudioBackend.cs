using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JamTemplate.Audio
{
    /// <summary>
    /// IAudioBackend implementation using Unity's built-in audio system.
    /// Handles both music (with crossfade via two AudioSources) and SFX (via object pool).
    /// Not a singleton — instantiated and owned by AudioManager.
    /// </summary>
    public class UnityAudioBackend : IAudioBackend
    {
        private GameObject owner;
        private MonoBehaviour coroutineRunner;

        // Music: two sources for crossfade
        private AudioSource musicSourceA;
        private AudioSource musicSourceB;
        private AudioSource activeMusicSource;
        private Coroutine musicFadeCoroutine;

        // SFX: pooled AudioSources
        private readonly List<AudioSource> sfxPool = new List<AudioSource>();
        private int initialPoolSize = 8;
        private GameObject sfxPoolParent;

        // Volumes
        private float masterVolume = 1f;
        private float musicVolume = 1f;
        private float sfxVolume = 1f;

        private bool isPaused;

        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float SFXVolume => sfxVolume;

        public void Initialize(GameObject owner)
        {
            this.owner = owner;
            coroutineRunner = owner.GetComponent<MonoBehaviour>();

            // Music sources
            musicSourceA = CreateAudioSource("MusicSource_A", true);
            musicSourceB = CreateAudioSource("MusicSource_B", true);
            activeMusicSource = musicSourceA;

            // SFX pool
            sfxPoolParent = new GameObject("SFXPool");
            sfxPoolParent.transform.SetParent(owner.transform);
            for (int i = 0; i < initialPoolSize; i++)
                sfxPool.Add(CreateSFXSource());
        }

        /// <summary>Set the initial pool size. Must be called before Initialize.</summary>
        public void SetPoolSize(int size) => initialPoolSize = size;

        public void Dispose()
        {
            if (musicFadeCoroutine != null && coroutineRunner != null)
                coroutineRunner.StopCoroutine(musicFadeCoroutine);
        }

        // ─────────────────────────────────────────────
        // MUSIC
        // ─────────────────────────────────────────────

        public void PlayMusic(AudioClip clip, float fadeTime = 1f)
        {
            if (clip == null) return;

            // If already playing this clip, do nothing
            if (activeMusicSource.clip == clip && activeMusicSource.isPlaying)
                return;

            // Crossfade
            var incoming = (activeMusicSource == musicSourceA) ? musicSourceB : musicSourceA;
            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.Play();

            if (musicFadeCoroutine != null)
                coroutineRunner.StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = coroutineRunner.StartCoroutine(
                CrossfadeCoroutine(activeMusicSource, incoming, fadeTime));

            activeMusicSource = incoming;
        }

        public void StopMusic(float fadeTime = 1f)
        {
            if (musicFadeCoroutine != null)
                coroutineRunner.StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = coroutineRunner.StartCoroutine(
                FadeOutCoroutine(activeMusicSource, fadeTime));
        }

        private IEnumerator CrossfadeCoroutine(AudioSource outgoing, AudioSource incoming, float duration)
        {
            float elapsed = 0f;
            float startVolumeOut = outgoing.volume;
            float targetVolume = musicVolume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                outgoing.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                incoming.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            outgoing.volume = 0f;
            outgoing.Stop();
            outgoing.clip = null;
            incoming.volume = targetVolume;
            musicFadeCoroutine = null;
        }

        private IEnumerator FadeOutCoroutine(AudioSource source, float duration)
        {
            float elapsed = 0f;
            float startVolume = source.volume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                source.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            source.volume = 0f;
            source.Stop();
            source.clip = null;
            musicFadeCoroutine = null;
        }

        // ─────────────────────────────────────────────
        // SFX
        // ─────────────────────────────────────────────

        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            var source = GetAvailableSFXSource();
            source.pitch = pitch;
            source.volume = sfxVolume * volume;
            source.PlayOneShot(clip);
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in sfxPool)
            {
                if (!source.isPlaying)
                    return source;
            }

            // All busy — expand pool
            var newSource = CreateSFXSource();
            sfxPool.Add(newSource);
            return newSource;
        }

        // ─────────────────────────────────────────────
        // VOLUME
        // ─────────────────────────────────────────────

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            AudioListener.volume = masterVolume;
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (activeMusicSource != null && activeMusicSource.isPlaying)
                activeMusicSource.volume = musicVolume;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        // ─────────────────────────────────────────────
        // PAUSE / RESUME
        // ─────────────────────────────────────────────

        public void PauseAudio()
        {
            if (isPaused) return;
            isPaused = true;

            if (musicSourceA.isPlaying) musicSourceA.Pause();
            if (musicSourceB.isPlaying) musicSourceB.Pause();

            foreach (var source in sfxPool)
            {
                if (source.isPlaying) source.Pause();
            }
        }

        public void ResumeAudio()
        {
            if (!isPaused) return;
            isPaused = false;

            musicSourceA.UnPause();
            musicSourceB.UnPause();

            foreach (var source in sfxPool)
            {
                source.UnPause();
            }
        }

        // ─────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────

        private AudioSource CreateAudioSource(string name, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(owner.transform);
            var source = go.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D
            return source;
        }

        private AudioSource CreateSFXSource()
        {
            var go = new GameObject($"SFX_{sfxPool.Count}");
            go.transform.SetParent(sfxPoolParent.transform);
            var source = go.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D
            return source;
        }
    }
}
