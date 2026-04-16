using UnityEngine;

namespace JamTemplate.HiScore
{
    /// <summary>
    /// Configuration for the hi-score system.
    /// Create via Assets > Create > JamTemplate > HiScore Config.
    /// </summary>
    [CreateAssetMenu(fileName = "HiScoreConfig", menuName = "JamTemplate/HiScore Config")]
    public class HiScoreConfig : ScriptableObject
    {
        [Tooltip("Maximum number of entries on the leaderboard.")]
        public int maxEntries = 10;

        [Tooltip("Save key used in SaveSystem for storing scores.")]
        public string saveKey = "hiscores";

        [Tooltip("True = highest score wins (default). False = lowest score wins (time-attack).")]
        public bool sortDescending = true;

        [Tooltip("Max characters for player name input (classic arcade: 3).")]
        public int maxNameLength = 3;
    }
}
