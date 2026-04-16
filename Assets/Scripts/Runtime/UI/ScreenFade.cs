using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.UI
{
    /// <summary>
    /// Full-screen fade overlay using a CanvasGroup.
    /// Attach to a Canvas with a full-screen Image child.
    /// Persists across scene loads via DontDestroyOnLoad.
    /// Uses unscaled time so fades work while paused.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFade : MonoBehaviour
    {
        public static ScreenFade Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;

        private CanvasGroup canvasGroup;
        private Image fadeImage;
        private Coroutine activeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            canvasGroup = GetComponent<CanvasGroup>();
            fadeImage = GetComponentInChildren<Image>();

            if (fadeImage != null)
                fadeImage.color = fadeColor;

            // Start fully transparent
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        /// <summary>
        /// Fade to black (alpha 0 -> 1). Screen becomes obscured.
        /// </summary>
        public void FadeOut(Action onComplete = null)
        {
            StartFade(0f, 1f, onComplete);
        }

        /// <summary>
        /// Fade from black (alpha 1 -> 0). Screen becomes visible.
        /// </summary>
        public void FadeIn(Action onComplete = null)
        {
            StartFade(1f, 0f, onComplete);
        }

        /// <summary>
        /// Instantly set the fade to fully opaque (black screen).
        /// </summary>
        public void SetOpaque()
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = false;
        }

        /// <summary>
        /// Instantly set the fade to fully transparent (clear screen).
        /// </summary>
        public void SetTransparent()
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public float FadeDuration => fadeDuration;

        private void StartFade(float from, float to, Action onComplete)
        {
            if (activeCoroutine != null)
                StopCoroutine(activeCoroutine);

            activeCoroutine = StartCoroutine(FadeCoroutine(from, to, onComplete));
        }

        private IEnumerator FadeCoroutine(float from, float to, Action onComplete)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = from;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }

            canvasGroup.alpha = to;
            canvasGroup.blocksRaycasts = to > 0.5f;
            canvasGroup.interactable = false;
            activeCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
