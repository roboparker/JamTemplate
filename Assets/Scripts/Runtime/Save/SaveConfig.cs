using UnityEngine;

namespace JamTemplate.Save
{
    public enum SaveBackendType
    {
        PlayerPrefs,
        JsonFile
    }

    /// <summary>
    /// Configuration asset for the save system.
    /// Create via Assets > Create > JamTemplate > Save Config.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveConfig", menuName = "JamTemplate/Save Config")]
    public class SaveConfig : ScriptableObject
    {
        [Tooltip("Which storage backend to use. JsonFile is NOT supported on WebGL.")]
        public SaveBackendType backendType = SaveBackendType.PlayerPrefs;

        [Tooltip("File name for JsonFile backend (stored in Application.persistentDataPath).")]
        public string saveFileName = "save.json";
    }
}
