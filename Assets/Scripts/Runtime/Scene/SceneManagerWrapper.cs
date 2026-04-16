using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Only one additive scene is visible at a time. Loading a new additive scene
    /// pushes the current one onto a stack. Back() restores the previous additive scene.
    /// This allows Pause → Settings → Back → Pause navigation.
    ///
    /// Persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    public class SceneManagerWrapper : MonoBehaviour
    {
        public static SceneManagerWrapper Instance { get; private set; }

        public event Action<string> OnSceneLoadStart;
        public event Action<string> OnSceneLoadComplete;

        private string currentAdditiveScene;
        private readonly Stack<string> additiveSceneStack = new Stack<string>();
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
        /// Clears any active additive scenes and the scene stack.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isTransitioning) return;
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        /// <summary>
        /// Load a scene additively on top of the current scene.
        /// If another additive scene is already open, it is pushed onto the stack
        /// (unloaded but remembered) so Back() can restore it.
        /// </summary>
        public void LoadSceneAdditive(string sceneName)
        {
            if (isTransitioning) return;
            StartCoroutine(LoadAdditiveCoroutine(sceneName));
        }

        /// <summary>
        /// Unload a specific additive scene by name.
        /// Does not restore previously stacked scenes — use Back() for that.
        /// </summary>
        public void UnloadAdditiveScene(string sceneName)
        {
            if (isTransitioning) return;
            if (currentAdditiveScene != sceneName) return;
            StartCoroutine(UnloadAdditiveCoroutine(sceneName, restorePrevious: false));
        }

        /// <summary>
        /// Unload the current additive scene and restore the previous one from the stack.
        /// If no previous scene exists, simply unloads the current additive scene.
        /// </summary>
        public void Back()
        {
            if (string.IsNullOrEmpty(currentAdditiveScene)) return;
            if (isTransitioning) return;
            StartCoroutine(UnloadAdditiveCoroutine(currentAdditiveScene, restorePrevious: true));
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

            // Clear additive scene state
            if (!string.IsNullOrEmpty(currentAdditiveScene))
            {
                var unloadOp = SceneManager.UnloadSceneAsync(currentAdditiveScene);
                if (unloadOp != null)
                    while (!unloadOp.isDone) yield return null;
            }
            currentAdditiveScene = null;
            additiveSceneStack.Clear();

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

            // Push current additive scene onto stack (unload it, remember it)
            if (!string.IsNullOrEmpty(currentAdditiveScene))
            {
                additiveSceneStack.Push(currentAdditiveScene);
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

        private IEnumerator UnloadAdditiveCoroutine(string sceneName, bool restorePrevious)
        {
            isTransitioning = true;

            var unloadOp = SceneManager.UnloadSceneAsync(sceneName);
            if (unloadOp != null)
                while (!unloadOp.isDone) yield return null;

            currentAdditiveScene = null;

            // Restore previous additive scene from the stack
            if (restorePrevious && additiveSceneStack.Count > 0)
            {
                string previousScene = additiveSceneStack.Pop();
                var loadOp = SceneManager.LoadSceneAsync(previousScene, LoadSceneMode.Additive);
                if (loadOp != null)
                    while (!loadOp.isDone) yield return null;
                currentAdditiveScene = previousScene;
            }

            isTransitioning = false;
        }
    }
}
