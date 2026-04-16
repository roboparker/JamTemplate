using System;
using UnityEngine;
using JamTemplate.Scene;

namespace JamTemplate.GameState
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Win
    }

    /// <summary>
    /// Enum-based state machine managing game flow.
    /// Handles pause (timeScale), game over, and win states.
    /// Loads/unloads additive UI scenes for Pause, Win, and Lose overlays.
    /// Persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public event Action OnPause;
        public event Action OnResume;
        public event Action OnGameOver;
        public event Action OnWin;
        public event Action<GameState> OnStateChanged;

        [SerializeField] private string pauseSceneName = "Pause";
        [SerializeField] private string winSceneName = "Win";
        [SerializeField] private string loseSceneName = "Lose";

        private GameState currentState = GameState.Playing;

        public GameState CurrentState => currentState;
        public bool IsPaused => currentState == GameState.Paused;
        public bool IsPlaying => currentState == GameState.Playing;

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
        /// Pause the game. Sets timeScale to 0 and loads the Pause scene additively.
        /// Only works from Playing state.
        /// </summary>
        public void Pause()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.Paused;
            Time.timeScale = 0f;

            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadSceneAdditive(pauseSceneName);

            OnPause?.Invoke();
            OnStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// Resume the game. Restores timeScale and unloads the Pause scene.
        /// Only works from Paused state.
        /// </summary>
        public void Resume()
        {
            if (currentState != GameState.Paused) return;

            currentState = GameState.Playing;
            Time.timeScale = 1f;

            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.UnloadAdditiveScene(pauseSceneName);

            OnResume?.Invoke();
            OnStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// Trigger game over. Loads the Lose scene additively.
        /// Only works from Playing state.
        /// </summary>
        public void TriggerGameOver()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.GameOver;
            Time.timeScale = 0f;

            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadSceneAdditive(loseSceneName);

            OnGameOver?.Invoke();
            OnStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// Trigger win. Loads the Win scene additively.
        /// Only works from Playing state.
        /// </summary>
        public void TriggerWin()
        {
            if (currentState != GameState.Playing) return;

            currentState = GameState.Win;
            Time.timeScale = 0f;

            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadSceneAdditive(winSceneName);

            OnWin?.Invoke();
            OnStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// Reset state to Playing and restore timeScale.
        /// Call this when restarting or returning to the title screen.
        /// </summary>
        public void ResetState()
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            OnStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// Quit the game. Uses EditorApplication.isPlaying in the editor,
        /// Application.Quit() in builds.
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
