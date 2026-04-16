using System;
using System.Collections.Generic;
using UnityEngine;
using JamTemplate.Save;

namespace JamTemplate.HiScore
{
    /// <summary>
    /// Singleton managing a ranked local leaderboard.
    /// Stores scores via SaveSystem. Supports both highest-first and lowest-first sorting.
    /// Display concerns belong in HiScoreDisplay — this class is data-only.
    /// </summary>
    public class HiScoreManager : MonoBehaviour
    {
        public static HiScoreManager Instance { get; private set; }

        [SerializeField] private HiScoreConfig config;

        private List<HiScoreEntry> scores = new List<HiScoreEntry>();

        /// <summary>Fired when scores change (submit, clear).</summary>
        public event Action OnScoresChanged;

        /// <summary>The last entry that was submitted (for highlighting in UI).</summary>
        public HiScoreEntry LastSubmitted { get; private set; }

        public HiScoreConfig Config => config;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScores();
        }

        /// <summary>
        /// Submit a new score. Adds to the list, re-sorts, trims to maxEntries.
        /// Returns the entry if it made the leaderboard, null otherwise.
        /// </summary>
        public HiScoreEntry SubmitScore(string playerName, float score)
        {
            var entry = new HiScoreEntry(
                playerName,
                score,
                DateTime.Now.ToShortDateString()
            );

            scores.Add(entry);
            SortScores();

            int maxEntries = config != null ? config.maxEntries : 10;
            if (scores.Count > maxEntries)
                scores.RemoveRange(maxEntries, scores.Count - maxEntries);

            // Check if the entry survived the trim
            if (scores.Contains(entry))
            {
                LastSubmitted = entry;
                SaveScores();
                OnScoresChanged?.Invoke();
                return entry;
            }

            // Didn't make the leaderboard
            return null;
        }

        /// <summary>Returns sorted list of scores (copy).</summary>
        public List<HiScoreEntry> GetScores()
        {
            return new List<HiScoreEntry>(scores);
        }

        /// <summary>Returns the top score entry, or null if empty.</summary>
        public HiScoreEntry GetTopScore()
        {
            return scores.Count > 0 ? scores[0] : null;
        }

        /// <summary>True if the given score would make it onto the leaderboard.</summary>
        public bool IsHiScore(float score)
        {
            int maxEntries = config != null ? config.maxEntries : 10;
            if (scores.Count < maxEntries) return true;

            bool descending = config == null || config.sortDescending;
            float worstScore = scores[scores.Count - 1].score;

            return descending ? score > worstScore : score < worstScore;
        }

        /// <summary>Clear all scores (debug / new game).</summary>
        public void ClearScores()
        {
            scores.Clear();
            LastSubmitted = null;
            SaveScores();
            OnScoresChanged?.Invoke();
        }

        /// <summary>Number of entries currently on the leaderboard.</summary>
        public int Count => scores.Count;

        private void SortScores()
        {
            bool descending = config == null || config.sortDescending;
            if (descending)
                scores.Sort((a, b) => b.score.CompareTo(a.score));
            else
                scores.Sort((a, b) => a.score.CompareTo(b.score));
        }

        // ─────────────────────────────────────────────
        // PERSISTENCE
        // ─────────────────────────────────────────────

        private void LoadScores()
        {
            if (SaveManager.Instance == null) return;

            string key = config != null ? config.saveKey : "hiscores";
            string json = SaveManager.Instance.LoadString(key, "");

            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<HiScoreListWrapper>(json);
                if (wrapper != null && wrapper.entries != null)
                {
                    scores = wrapper.entries;
                    SortScores();
                }
            }
        }

        private void SaveScores()
        {
            if (SaveManager.Instance == null) return;

            string key = config != null ? config.saveKey : "hiscores";
            var wrapper = new HiScoreListWrapper { entries = scores };
            string json = JsonUtility.ToJson(wrapper);
            SaveManager.Instance.Save(key, json);
        }

        /// <summary>
        /// Wrapper for JsonUtility serialization (can't serialize List directly).
        /// </summary>
        [Serializable]
        private class HiScoreListWrapper
        {
            public List<HiScoreEntry> entries = new List<HiScoreEntry>();
        }
    }
}
