using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.HiScore
{
    /// <summary>
    /// UI controller that reads from HiScoreManager and renders the leaderboard.
    /// Attach to a prefab or scene object with a ScrollRect.
    /// Call Refresh() to update, or it auto-refreshes on enable and when scores change.
    /// </summary>
    public class HiScoreDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform rowParent;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Row Colors")]
        [SerializeField] private Color topRankColor = new Color(1f, 0.84f, 0f, 1f); // Gold
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(0.5f, 1f, 0.5f, 1f); // Green for last submitted

        [Header("Layout")]
        [SerializeField] private int fontSize = 18;

        private readonly List<GameObject> rowInstances = new List<GameObject>();

        private void OnEnable()
        {
            Refresh();

            if (HiScoreManager.Instance != null)
                HiScoreManager.Instance.OnScoresChanged += Refresh;
        }

        private void OnDisable()
        {
            if (HiScoreManager.Instance != null)
                HiScoreManager.Instance.OnScoresChanged -= Refresh;
        }

        /// <summary>
        /// Rebuild the leaderboard display from HiScoreManager data.
        /// </summary>
        public void Refresh()
        {
            // Clear existing rows
            foreach (var row in rowInstances)
                Destroy(row);
            rowInstances.Clear();

            if (HiScoreManager.Instance == null || rowParent == null) return;

            var scores = HiScoreManager.Instance.GetScores();
            var lastSubmitted = HiScoreManager.Instance.LastSubmitted;

            for (int i = 0; i < scores.Count; i++)
            {
                var entry = scores[i];
                var row = CreateRow(i + 1, entry, entry == lastSubmitted);
                rowInstances.Add(row);
            }

            // Show empty message if no scores
            if (scores.Count == 0)
            {
                var emptyRow = CreateTextRow("No scores yet", normalColor);
                rowInstances.Add(emptyRow);
            }
        }

        private GameObject CreateRow(int rank, HiScoreEntry entry, bool isLastSubmitted)
        {
            Color color;
            if (isLastSubmitted)
                color = highlightColor;
            else if (rank == 1)
                color = topRankColor;
            else
                color = normalColor;

            string text = $"{rank}. {entry.playerName}  —  {entry.score:F0}  ({entry.date})";
            return CreateTextRow(text, color);
        }

        private GameObject CreateTextRow(string text, Color color)
        {
            var go = new GameObject("ScoreRow", typeof(RectTransform));
            go.transform.SetParent(rowParent, false);

            var textComponent = go.AddComponent<Text>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.alignment = TextAnchor.MiddleLeft;
            textComponent.color = color;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (textComponent.font == null)
                textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);

            return go;
        }
    }
}
