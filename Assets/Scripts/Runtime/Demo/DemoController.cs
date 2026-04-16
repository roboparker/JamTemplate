using UnityEngine;
using UnityEngine.UI;
using JamTemplate.GameState;
using JamTemplate.Scene;

namespace JamTemplate.Demo
{
    /// <summary>
    /// Demo gameplay scene controller that exercises all core systems.
    /// Replace or extend this when starting a jam.
    /// </summary>
    public class DemoController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button winButton;
        [SerializeField] private Button loseButton;

        private void Start()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);
            if (winButton != null)
                winButton.onClick.AddListener(OnWinClicked);
            if (loseButton != null)
                loseButton.onClick.AddListener(OnLoseClicked);
        }

        private void Update()
        {
            // Pause on Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameStateManager.Instance != null)
                {
                    if (GameStateManager.Instance.IsPlaying)
                        GameStateManager.Instance.Pause();
                    else if (GameStateManager.Instance.IsPaused)
                        GameStateManager.Instance.Resume();
                }
            }
        }

        public void OnPauseClicked()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.IsPlaying)
                GameStateManager.Instance.Pause();
        }

        public void OnWinClicked()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.TriggerWin();
        }

        public void OnLoseClicked()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.TriggerGameOver();
        }
    }
}
