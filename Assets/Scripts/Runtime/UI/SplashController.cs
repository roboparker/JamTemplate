using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace JamTemplate.UI
{
    /// <summary>
    /// Controls the Splash scene (build index 0).
    /// Plays a fullscreen video via VideoPlayer + RawImage (WebGL-compatible).
    /// Skippable on any input. Auto-advances when video ends.
    /// Falls back gracefully if no video clip is assigned or video fails.
    /// </summary>
    public class SplashController : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage videoImage;

        [Header("Settings")]
        [SerializeField] private string nextSceneName = "Title";
        [SerializeField] private bool skippable = true;
        [SerializeField] private float fallbackDisplayTime = 2f;

        [Header("Fade")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private bool isTransitioning;

        private void Start()
        {
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = 1f;

            if (videoPlayer != null && videoPlayer.clip != null)
            {
                videoPlayer.loopPointReached += OnVideoEnd;
                videoPlayer.errorReceived += OnVideoError;
                videoPlayer.prepareCompleted += OnVideoPrepared;
                videoPlayer.Prepare();
            }
            else
            {
                // No video — show splash briefly then advance
                StartCoroutine(FallbackSplash());
            }
        }

        private void Update()
        {
            if (skippable && !isTransitioning && Input.anyKeyDown)
            {
                TransitionToNextScene();
            }
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            if (videoImage != null)
                videoImage.texture = vp.texture;
            vp.Play();

            // Fade in
            StartCoroutine(Fade(1f, 0f));
        }

        private void OnVideoEnd(VideoPlayer vp)
        {
            TransitionToNextScene();
        }

        private void OnVideoError(VideoPlayer vp, string message)
        {
            Debug.LogWarning($"[Splash] Video error: {message}. Falling back.");
            TransitionToNextScene();
        }

        private IEnumerator FallbackSplash()
        {
            // Fade in
            yield return Fade(1f, 0f);

            // Wait
            yield return new WaitForSeconds(fallbackDisplayTime);

            // Advance
            TransitionToNextScene();
        }

        private void TransitionToNextScene()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            StartCoroutine(TransitionCoroutine());
        }

        private IEnumerator TransitionCoroutine()
        {
            yield return Fade(0f, 1f);
            SceneManager.LoadScene(nextSceneName);
        }

        private IEnumerator Fade(float from, float to)
        {
            if (fadeCanvasGroup == null) yield break;

            fadeCanvasGroup.alpha = from;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }

            fadeCanvasGroup.alpha = to;
        }
    }
}
