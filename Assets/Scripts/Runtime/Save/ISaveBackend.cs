namespace JamTemplate.Save
{
    /// <summary>
    /// Abstraction for save data storage.
    /// Implementations handle the actual persistence mechanism.
    /// </summary>
    public interface ISaveBackend
    {
        void Save(string key, int value);
        void Save(string key, float value);
        void Save(string key, string value);

        int LoadInt(string key, int defaultValue = 0);
        float LoadFloat(string key, float defaultValue = 0f);
        string LoadString(string key, string defaultValue = "");

        bool HasKey(string key);
        void DeleteKey(string key);
        void DeleteAll();
    }
}
