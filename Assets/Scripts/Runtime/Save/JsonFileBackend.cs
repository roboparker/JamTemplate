using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace JamTemplate.Save
{
    /// <summary>
    /// Save backend that writes JSON to Application.persistentDataPath.
    /// NOT suitable for WebGL — use PlayerPrefsBackend instead.
    /// Data is stored as parallel key/value lists (JsonUtility can't serialize Dictionary).
    /// </summary>
    public class JsonFileBackend : ISaveBackend
    {
        private readonly string filePath;
        private SaveFileData data;

        // Runtime dictionaries (rebuilt from serialized lists on load)
        private Dictionary<string, int> intValues = new Dictionary<string, int>();
        private Dictionary<string, float> floatValues = new Dictionary<string, float>();
        private Dictionary<string, string> stringValues = new Dictionary<string, string>();

        public JsonFileBackend(string fileName = "save.json")
        {
            filePath = Path.Combine(Application.persistentDataPath, fileName);
            LoadFromDisk();
        }

        public void Save(string key, int value)
        {
            intValues[key] = value;
            WriteToDisk();
        }

        public void Save(string key, float value)
        {
            floatValues[key] = value;
            WriteToDisk();
        }

        public void Save(string key, string value)
        {
            stringValues[key] = value;
            WriteToDisk();
        }

        public int LoadInt(string key, int defaultValue = 0)
        {
            return intValues.TryGetValue(key, out int value) ? value : defaultValue;
        }

        public float LoadFloat(string key, float defaultValue = 0f)
        {
            return floatValues.TryGetValue(key, out float value) ? value : defaultValue;
        }

        public string LoadString(string key, string defaultValue = "")
        {
            return stringValues.TryGetValue(key, out string value) ? value : defaultValue;
        }

        public bool HasKey(string key)
        {
            return intValues.ContainsKey(key)
                || floatValues.ContainsKey(key)
                || stringValues.ContainsKey(key);
        }

        public void DeleteKey(string key)
        {
            intValues.Remove(key);
            floatValues.Remove(key);
            stringValues.Remove(key);
            WriteToDisk();
        }

        public void DeleteAll()
        {
            intValues.Clear();
            floatValues.Clear();
            stringValues.Clear();
            WriteToDisk();
        }

        private void LoadFromDisk()
        {
            if (!File.Exists(filePath))
            {
                data = new SaveFileData();
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                data = JsonUtility.FromJson<SaveFileData>(json) ?? new SaveFileData();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Failed to load save file: {e.Message}");
                data = new SaveFileData();
            }

            // Rebuild dictionaries from the deserialized parallel lists
            intValues.Clear();
            for (int i = 0; i < data.intKeys.Count && i < data.intVals.Count; i++)
                intValues[data.intKeys[i]] = data.intVals[i];

            floatValues.Clear();
            for (int i = 0; i < data.floatKeys.Count && i < data.floatVals.Count; i++)
                floatValues[data.floatKeys[i]] = data.floatVals[i];

            stringValues.Clear();
            for (int i = 0; i < data.stringKeys.Count && i < data.stringVals.Count; i++)
                stringValues[data.stringKeys[i]] = data.stringVals[i];
        }

        private void WriteToDisk()
        {
            // Rebuild parallel lists from dictionaries before serialization
            data.intKeys.Clear(); data.intVals.Clear();
            foreach (var kvp in intValues) { data.intKeys.Add(kvp.Key); data.intVals.Add(kvp.Value); }

            data.floatKeys.Clear(); data.floatVals.Clear();
            foreach (var kvp in floatValues) { data.floatKeys.Add(kvp.Key); data.floatVals.Add(kvp.Value); }

            data.stringKeys.Clear(); data.stringVals.Clear();
            foreach (var kvp in stringValues) { data.stringKeys.Add(kvp.Key); data.stringVals.Add(kvp.Value); }

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(filePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to write save file: {e.Message}");
            }
        }

        /// <summary>
        /// Serializable container for JsonUtility.
        /// Stores typed key-value pairs as parallel lists.
        /// </summary>
        [System.Serializable]
        private class SaveFileData
        {
            public List<string> intKeys = new List<string>();
            public List<int> intVals = new List<int>();
            public List<string> floatKeys = new List<string>();
            public List<float> floatVals = new List<float>();
            public List<string> stringKeys = new List<string>();
            public List<string> stringVals = new List<string>();
        }
    }
}
