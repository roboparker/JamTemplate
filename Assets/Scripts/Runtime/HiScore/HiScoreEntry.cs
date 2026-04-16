namespace JamTemplate.HiScore
{
    /// <summary>
    /// A single hi-score entry. Serializable for JSON storage via SaveSystem.
    /// </summary>
    [System.Serializable]
    public class HiScoreEntry
    {
        public string playerName;
        public float score;
        public string date;

        public HiScoreEntry() { }

        public HiScoreEntry(string playerName, float score, string date)
        {
            this.playerName = playerName;
            this.score = score;
            this.date = date;
        }
    }
}
