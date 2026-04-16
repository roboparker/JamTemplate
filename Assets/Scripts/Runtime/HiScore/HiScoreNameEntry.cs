using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.HiScore
{
    /// <summary>
    /// Name entry field for submitting a hi-score.
    /// Works with InputField (legacy UI). Add to Win/Lose scenes.
    /// Shows/hides based on whether the current score qualifies.
    /// </summary>
    public class HiScoreNameEntry : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InputField nameInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Text feedbackText;
        [SerializeField] private GameObject entryPanel;

        [Header("Settings")]
        [SerializeField] private float currentScore;

        private bool hasSubmitted;

        private void Start()
        {
            if (submitButton != null)
                submitButton.onClick.AddListener(OnSubmitClicked);

            // Enforce max name length from config
            if (nameInput != null && HiScoreManager.Instance != null && HiScoreManager.Instance.Config != null)
                nameInput.characterLimit = HiScoreManager.Instance.Config.maxNameLength;

            UpdateVisibility();
        }

        /// <summary>
        /// Set the score to submit. Call this from the game when the round ends.
        /// </summary>
        public void SetScore(float score)
        {
            currentScore = score;
            hasSubmitted = false;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (entryPanel == null) return;

            bool show = !hasSubmitted
                && HiScoreManager.Instance != null
                && HiScoreManager.Instance.IsHiScore(currentScore);

            entryPanel.SetActive(show);

            if (feedbackText != null)
            {
                if (show)
                    feedbackText.text = "New high score! Enter your name:";
                else if (hasSubmitted)
                    feedbackText.text = "Score submitted!";
                else
                    feedbackText.text = "";
            }
        }

        public void OnSubmitClicked()
        {
            if (hasSubmitted) return;
            if (HiScoreManager.Instance == null) return;

            string playerName = nameInput != null ? nameInput.text.Trim() : "???";
            if (string.IsNullOrEmpty(playerName))
                playerName = "???";

            HiScoreManager.Instance.SubmitScore(playerName, currentScore);
            hasSubmitted = true;
            UpdateVisibility();
        }
    }
}
