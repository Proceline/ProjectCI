using System;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Saving.Data
{
    /// <summary>
    /// Simple save slot information for display in load menu
    /// Contains only essential metadata, not full save data
    /// </summary>
    [Serializable]
    public class PvSaveDetails
    {
        [SerializeField] private string saveSlotName; // User-friendly name like "default"
        [SerializeField] private string saveFolderGuid; // GUID folder name
        [SerializeField] private string saveTime; // ISO format timestamp
        [SerializeField] private float totalPlayTime; // Total play time in seconds
        
        public string SaveSlotName
        {
            get => saveSlotName;
            set => saveSlotName = value;
        }
        
        public string SaveFolderGuid
        {
            get => saveFolderGuid;
            set => saveFolderGuid = value;
        }
        
        public string SaveTime
        {
            get => saveTime;
            set => saveTime = value;
        }
        
        public float TotalPlayTime
        {
            get => totalPlayTime;
            set => totalPlayTime = value;
        }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public PvSaveDetails()
        {
            saveSlotName = "default";
            saveFolderGuid = Guid.NewGuid().ToString();
            saveTime = DateTime.UtcNow.ToString("O"); // ISO 8601 format
            totalPlayTime = 0f;
        }
        
        /// <summary>
        /// Constructor with slot name
        /// </summary>
        public PvSaveDetails(string slotName)
        {
            saveSlotName = slotName;
            saveFolderGuid = Guid.NewGuid().ToString();
            saveTime = DateTime.UtcNow.ToString("O");
            totalPlayTime = 0f;
        }
        
        /// <summary>
        /// Update save time to current time
        /// </summary>
        public void UpdateSaveTime()
        {
            saveTime = DateTime.UtcNow.ToString("O");
        }
    }
}
