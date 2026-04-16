using UnityEngine;
using UnityEngine.UI;
using JamTemplate.GameState;
using JamTemplate.Scene;

namespace JamTemplate.UI
{
    /// <summary>
    /// Controls the Pause additive scene.
    /// Loaded by GameStateManager.Pause(), unloaded by Resume().
    /// Buttons: Resume, Restart, Settings, Main Menu, Quit.
    /// Quit button hidden on WebGL.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("Settings")]
        [SerializeField] private string gameplaySceneName = "Demo";
        [SerializeField] private bool hideQuitButtonOnWebGL = true;

        private void Start()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);

                if (hideQuitButtonOnWebGL && Application.platform == RuntimePlatform.WebGLPlayer)
                    quitButton.gameObject.SetActive(false);
            }
        }

        public void OnResumeClicked()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.Resume();
        }

        public void OnRestartClicked()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.ResetState();

            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadScene(gameplaySceneName);
        }

        public void OnSettingsClicked()
        {
            // Opens Settings additively — Pause is pushed to the scene stack
            // and will be restored when Settings calls Back()
            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadSceneAdditive("Settings");
        }

        public void OnMainMenuClicked()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.ResetState();

            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadScene("Title");
        }

        public void OnQuitClicked()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.QuitGame();
        }
    }
}
