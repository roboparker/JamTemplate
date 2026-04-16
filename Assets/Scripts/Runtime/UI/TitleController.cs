using UnityEngine;
using UnityEngine.UI;
using JamTemplate.GameState;
using JamTemplate.Scene;

namespace JamTemplate.UI
{
    /// <summary>
    /// Controls the Title scene UI.
    /// Buttons: Play, Settings, Credits, Quit.
    /// Quit button is hidden on WebGL (Application.Quit() is a no-op).
    /// </summary>
    public class TitleController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("Settings")]
        [SerializeField] private string gameplaySceneName = "Demo";
        [SerializeField] private bool hideQuitButtonOnWebGL = true;

        private void Start()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsClicked);
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);

                if (hideQuitButtonOnWebGL && Application.platform == RuntimePlatform.WebGLPlayer)
                    quitButton.gameObject.SetActive(false);
            }
        }

        public void OnPlayClicked()
        {
            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadScene(gameplaySceneName);
        }

        public void OnSettingsClicked()
        {
            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadSceneAdditive("Settings");
        }

        public void OnCreditsClicked()
        {
            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.LoadSceneAdditive("Credits");
        }

        public void OnQuitClicked()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.QuitGame();
        }
    }
}
