using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using JamTemplate.UI;

namespace JamTemplate.Scene
{
    /// <summary>
    /// Singleton scene manager with two load modes:
    /// - Single: full scene swap with ScreenFade transition
    /// - Additive: load a UI scene on top (Settings, Pause, Win, Lose, Credits)
    ///
    /// Only one additive scene is open at a time — loading a new one unloads the previous.
    /// Persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    public class SceneManagerWrapper : MonoBehaviour
    {
        public static SceneManagerWrapper Instance { get; private set; }

        public event Action<string> OnSceneLoadStart;
        public event Action<string> OnSceneLoadComplete;

        private string currentAdditiveScene;
        private bool isTransitioning;

        /// <summary>True while a scene load/unload is in progress.</summary>
        public bool IsTransitioning => isTransitioning;

        /// <summary>Name of the currently loaded additive scene, or null.</summary>
        public string CurrentAdditiveScene => currentAdditiveScene;

        /// <summary>True if an additive scene is currently loaded.</summary>
        public bool HasAdditiveScene => !string.IsNullOrEmpty(currentAdditiveScene);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Load a scene in Single mode with a fade transition.
        /// Unloads any active additive scene first.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isTransitioning) return;
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        /// <summary>
        /// Load a scene additively on top of the current scene.
        /// If another additive scene is already open, it is unloaded first.
        /// </summary>
        public void LoadSceneAdditive(string sceneName)
        {
            if (isTransitioning) return;
            StartCoroutine(LoadAdditiveCoroutine(sceneName));
        }

        /// <summary>
        /// Unload a specific additive scene by name.
        /// </summary>
        public void UnloadAdditiveScene(string sceneName)
        {
            if (isTransitioning) return;
            if (currentAdditiveScene != sceneName) return;
            StartCoroutine(UnloadAdditiveCoroutine(sceneName));
        }

        /// <summary>
        /// Unload the current additive scene (if any).
        /// </summary>
        public void Back()
        {
            if (string.IsNullOrEmpty(currentAdditiveScene)) return;
            UnloadAdditiveScene(currentAdditiveScene);
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            isTransitioning = true;
            OnSceneLoadStart?.Invoke(sceneName);

            // Fade to black
            if (ScreenFade.Instance != null)
            {
                bool fadeDone = false;
                ScreenFade.Instance.FadeOut(() => fadeDone = true);
                while (!fadeDone) yield return null;
            }

            // Unload additive scene if one is open
            if (!string.IsNullOrEmpty(currentAdditiveScene))
            {
                var unloadOp = SceneManager.UnloadSceneAsync(currentAdditiveScene);
                if (unloadOp != null)
                    while (!unloadOp.isDone) yield return null;
                currentAdditiveScene = null;
            }

            // Load the new scene
            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (loadOp != null)
                while (!loadOp.isDone) yield return null;

            // Fade from black
            if (ScreenFade.Instance != null)
            {
                bool fadeDone = false;
                ScreenFade.Instance.FadeIn(() => fadeDone = true);
                while (!fadeDone) yield return null;
            }

            isTransitioning = false;
            OnSceneLoadComplete?.Invoke(sceneName);
        }

        private IEnumerator LoadAdditiveCoroutine(string sceneName)
        {
            isTransitioning = true;
            OnSceneLoadStart?.Invoke(sceneName);

            // Unload current additive scene first
            if (!string.IsNullOrEmpty(currentAdditiveScene))
            {
                var unloadOp = SceneManager.UnloadSceneAsync(currentAdditiveScene);
                if (unloadOp != null)
                    while (!unloadOp.isDone) yield return null;
                currentAdditiveScene = null;
            }

            // Load new additive scene
            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOp != null)
                while (!loadOp.isDone) yield return null;

            currentAdditiveScene = sceneName;
            isTransitioning = false;
            OnSceneLoadComplete?.Invoke(sceneName);
        }

        private IEnumerator UnloadAdditiveCoroutine(string sceneName)
        {
            isTransitioning = true;

            var unloadOp = SceneManager.UnloadSceneAsync(sceneName);
            if (unloadOp != null)
                while (!unloadOp.isDone) yield return null;

            currentAdditiveScene = null;
            isTransitioning = false;
        }
    }
}
