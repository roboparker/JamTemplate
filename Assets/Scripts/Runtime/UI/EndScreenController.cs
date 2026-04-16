using UnityEngine;
using UnityEngine.UI;
using JamTemplate.GameState;
using JamTemplate.Scene;

namespace JamTemplate.UI
{
    /// <summary>
    /// Shared controller for Win and Lose additive scenes.
    /// Parameterized by Inspector settings (header text, button labels).
    /// Loaded by GameStateManager.TriggerWin() / TriggerGameOver().
    /// </summary>
    public class EndScreenController : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private Text headerText;
        [SerializeField] private Text messageText;
        [SerializeField] private string headerString = "Game Over";
        [SerializeField] private string messageString = "Better luck next time!";

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Text retryButtonText;
        [SerializeField] private string retryButtonLabel = "Try Again";

        [Header("Settings")]
        [SerializeField] private string gameplaySceneName = "Demo";

        private void Start()
        {
            if (headerText != null)
                headerText.text = headerString;
            if (messageText != null)
                messageText.text = messageString;
            if (retryButtonText != null)
                retryButtonText.text = retryButtonLabel;

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        public void OnRetryClicked()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.ResetState();

            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadScene(gameplaySceneName);
        }

        public void OnMainMenuClicked()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.ResetState();

            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadScene("Title");
        }
    }
}
