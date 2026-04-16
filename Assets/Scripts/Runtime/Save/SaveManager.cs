using UnityEngine;

namespace JamTemplate.Save
{
    /// <summary>
    /// Singleton facade for the save system.
    /// Delegates to the configured ISaveBackend.
    /// Auto-falls back to PlayerPrefs on WebGL regardless of config.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [SerializeField] private SaveConfig config;

        private ISaveBackend backend;

        public ISaveBackend Backend => backend;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBackend();
        }

        private void InitializeBackend()
        {
            if (config == null)
            {
                Debug.LogWarning("[SaveManager] No SaveConfig assigned, defaulting to PlayerPrefs.");
                backend = new PlayerPrefsBackend();
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            if (config.backendType == SaveBackendType.JsonFile)
            {
                Debug.LogWarning("[SaveManager] JsonFile backend not supported on WebGL. Falling back to PlayerPrefs.");
            }
            backend = new PlayerPrefsBackend();
#else
            switch (config.backendType)
            {
                case SaveBackendType.JsonFile:
                    backend = new JsonFileBackend(config.saveFileName);
                    break;
                case SaveBackendType.PlayerPrefs:
                default:
                    backend = new PlayerPrefsBackend();
                    break;
            }
#endif
        }

        public void Save(string key, int value) => backend.Save(key, value);
        public void Save(string key, float value) => backend.Save(key, value);
        public void Save(string key, string value) => backend.Save(key, value);

        public int LoadInt(string key, int defaultValue = 0) => backend.LoadInt(key, defaultValue);
        public float LoadFloat(string key, float defaultValue = 0f) => backend.LoadFloat(key, defaultValue);
        public string LoadString(string key, string defaultValue = "") => backend.LoadString(key, defaultValue);

        public bool HasKey(string key) => backend.HasKey(key);
        public void DeleteKey(string key) => backend.DeleteKey(key);
        public void DeleteAll() => backend.DeleteAll();
    }
}
