using UnityEngine;

namespace JamTemplate.UI
{
    /// <summary>
    /// Data-driven credits content. Create via Assets > Create > JamTemplate > Credits Data.
    /// Swap per jam by replacing the asset — no code changes needed.
    /// </summary>
    [CreateAssetMenu(fileName = "CreditsData", menuName = "JamTemplate/Credits Data")]
    public class CreditsData : ScriptableObject
    {
        public string gameTitle = "Game Title";
        public CreditSection[] sections = new CreditSection[]
        {
            new CreditSection
            {
                heading = "Development",
                entries = new string[] { "Your Name" }
            },
            new CreditSection
            {
                heading = "Art",
                entries = new string[] { "Artist Name" }
            },
            new CreditSection
            {
                heading = "Audio",
                entries = new string[] { "Composer Name" }
            },
            new CreditSection
            {
                heading = "Tools",
                entries = new string[] { "Unity 6", "Wwise (optional)" }
            },
            new CreditSection
            {
                heading = "Special Thanks",
                entries = new string[] { "Game Jam Organizers", "Playtesters" }
            }
        };
    }

    [System.Serializable]
    public class CreditSection
    {
        public string heading;
        [TextArea(1, 5)]
        public string[] entries;
    }
}
