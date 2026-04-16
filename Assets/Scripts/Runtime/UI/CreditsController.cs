using UnityEngine;
using UnityEngine.UI;
using JamTemplate.Scene;

namespace JamTemplate.UI
{
    /// <summary>
    /// Controls the Credits additive scene.
    /// Populates scrolling credits from a CreditsData ScriptableObject.
    /// Back button unloads the scene.
    /// </summary>
    public class CreditsController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CreditsData creditsData;

        [Header("UI References")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private Button backButton;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Scroll Settings")]
        [SerializeField] private bool autoScroll = true;
        [SerializeField] private float scrollSpeed = 30f;

        [Header("Prefab References (optional)")]
        [SerializeField] private GameObject headingPrefab;
        [SerializeField] private GameObject entryPrefab;

        private bool userInteracted;

        private void Start()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (creditsData != null)
                PopulateCredits();
        }

        private void Update()
        {
            if (autoScroll && !userInteracted && scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.unscaledDeltaTime * 0.001f;
                if (scrollRect.verticalNormalizedPosition <= 0f)
                    scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>
        /// Called by ScrollRect's OnValueChanged or input detection to pause auto-scroll.
        /// </summary>
        public void OnUserScroll()
        {
            userInteracted = true;
        }

        private void PopulateCredits()
        {
            if (contentParent == null) return;

            // Clear existing children (except the template objects)
            foreach (Transform child in contentParent)
                Destroy(child.gameObject);

            // Game title
            CreateTextElement(creditsData.gameTitle, 32, FontStyle.Bold);
            CreateSpacer(20f);

            // Sections
            foreach (var section in creditsData.sections)
            {
                CreateTextElement(section.heading, 24, FontStyle.Bold);
                CreateSpacer(5f);

                foreach (var entry in section.entries)
                {
                    CreateTextElement(entry, 18, FontStyle.Normal);
                }

                CreateSpacer(20f);
            }
        }

        private void CreateTextElement(string text, int fontSize, FontStyle style)
        {
            var go = new GameObject("CreditText", typeof(RectTransform));
            go.transform.SetParent(contentParent, false);

            var textComponent = go.AddComponent<Text>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = style;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.white;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (textComponent.font == null)
                textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var layout = go.AddComponent<ContentSizeFitter>();
            layout.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
        }

        private void CreateSpacer(float height)
        {
            var go = new GameObject("Spacer", typeof(RectTransform));
            go.transform.SetParent(contentParent, false);

            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = height;
            layoutElement.flexibleWidth = 1f;
        }

        public void OnBackClicked()
        {
            if (SceneManagerWrapper.Instance != null)
                SceneManagerWrapper.Instance.Back();
        }
    }
}
