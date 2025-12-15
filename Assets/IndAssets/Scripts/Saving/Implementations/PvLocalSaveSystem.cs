using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Saving.Data;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.Saving.Implementations
{
    /// <summary>
    /// Local file-based save system using folder structure
    /// Each save is stored in a GUID-named folder containing details.json and saveData.json
    /// </summary>
    public class PvLocalSaveSystem : IPvSaveSystem
    {
        private const string SaveDirectoryName = "Saves";
        private const string DetailsFileName = "details.json";
        private const string SaveDataFileName = "saveData.json";
        
        private string _saveDirectoryPath;
        private bool _isInitialized;
        
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Initialize the local save system
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                _saveDirectoryPath = Path.Combine(Application.persistentDataPath, SaveDirectoryName);
                
                if (!Directory.Exists(_saveDirectoryPath))
                {
                    Directory.CreateDirectory(_saveDirectoryPath);
                }
                
                await Awaitable.WaitForSecondsAsync(0.5f);
                _isInitialized = true;
                Debug.Log($"Local save system initialized. Save directory: {_saveDirectoryPath}");
                return true;
            }
            catch (Exception ex)
            {
                await Awaitable.WaitForSecondsAsync(0.5f);
                Debug.LogError($"Failed to initialize local save system: {ex.Message}");
                _isInitialized = false;
                return false;
            }
        }
        
        /// <summary>
        /// Save game data to local folder structure
        /// </summary>
        public async Task<bool> SaveAsync(PvSaveData saveData, string slotName = "default")
        {
            if (!_isInitialized)
            {
                Debug.LogError("Save system not initialized. Call InitializeAsync first.");
                return false;
            }
            
            if (saveData == null)
            {
                Debug.LogError("Save data is null");
                return false;
            }
            
            try
            {
                // Find existing save folder by slot name, or create new one
                string saveFolderGuid = await FindSaveFolderBySlotNameAsync(slotName);
                if (string.IsNullOrEmpty(saveFolderGuid))
                {
                    saveFolderGuid = Guid.NewGuid().ToString();
                }
                
                string saveFolderPath = GetSaveFolderPath(saveFolderGuid);
                
                // Create folder if it doesn't exist
                if (!Directory.Exists(saveFolderPath))
                {
                    Directory.CreateDirectory(saveFolderPath);
                }
                
                // Create or update save details
                PvSaveDetails details = await LoadDetailsAsync(saveFolderGuid);
                if (details == null)
                {
                    details = new PvSaveDetails(slotName);
                    details.SaveFolderGuid = saveFolderGuid;
                }
                else
                {
                    details.SaveSlotName = slotName;
                }
                details.UpdateSaveTime();
                // TODO: Update totalPlayTime from saveData if available
                
                // Save details.json
                string detailsJson = JsonUtility.ToJson(details, true);
                string detailsPath = GetDetailsFilePath(saveFolderGuid);
                string tempDetailsPath = detailsPath + ".tmp";
                await File.WriteAllTextAsync(tempDetailsPath, detailsJson);
                if (File.Exists(detailsPath))
                {
                    File.Delete(detailsPath);
                }
                File.Move(tempDetailsPath, detailsPath);
                
                // Save saveData.json
                string saveDataJson = JsonUtility.ToJson(saveData, true);
                string saveDataPath = GetSaveDataFilePath(saveFolderGuid);
                string tempSaveDataPath = saveDataPath + ".tmp";
                await File.WriteAllTextAsync(tempSaveDataPath, saveDataJson);
                if (File.Exists(saveDataPath))
                {
                    File.Delete(saveDataPath);
                }
                File.Move(tempSaveDataPath, saveDataPath);
                
                Debug.Log($"Game data saved to folder: {saveFolderPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save game data: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Load game data by slot name (finds GUID folder first)
        /// </summary>
        public async Task<PvSaveData> LoadAsync(string slotName = "default")
        {
            if (!_isInitialized)
            {
                Debug.LogError("Save system not initialized. Call InitializeAsync first.");
                return null;
            }
            
            try
            {
                string saveFolderGuid = await FindSaveFolderBySlotNameAsync(slotName);
                if (string.IsNullOrEmpty(saveFolderGuid))
                {
                    Debug.LogWarning($"Save slot not found: {slotName}");
                    return null;
                }
                
                return await LoadByGuidAsync(saveFolderGuid);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load game data: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Load game data by save folder GUID
        /// </summary>
        public async Task<PvSaveData> LoadByGuidAsync(string saveFolderGuid)
        {
            if (!_isInitialized)
            {
                Debug.LogError("Save system not initialized. Call InitializeAsync first.");
                return null;
            }
            
            try
            {
                string saveDataPath = GetSaveDataFilePath(saveFolderGuid);
                
                if (!File.Exists(saveDataPath))
                {
                    Debug.LogWarning($"Save data file not found: {saveDataPath}");
                    return null;
                }
                
                string json = await File.ReadAllTextAsync(saveDataPath);
                PvSaveData saveData = JsonUtility.FromJson<PvSaveData>(json);
                
                if (saveData == null)
                {
                    Debug.LogError("Failed to deserialize save data");
                    return null;
                }
                
                Debug.Log($"Game data loaded from: {saveDataPath}");
                return saveData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load game data: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Check if a save slot exists
        /// </summary>
        public async Task<bool> SaveExistsAsync(string slotName = "default")
        {
            if (!_isInitialized)
            {
                return false;
            }
            
            try
            {
                string saveFolderGuid = await FindSaveFolderBySlotNameAsync(slotName);
                if (string.IsNullOrEmpty(saveFolderGuid))
                {
                    return false;
                }
                
                string saveDataPath = GetSaveDataFilePath(saveFolderGuid);
                return File.Exists(saveDataPath);
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Delete a save slot (entire folder)
        /// </summary>
        public async Task<bool> DeleteSaveAsync(string slotName = "default")
        {
            if (!_isInitialized)
            {
                Debug.LogError("Save system not initialized. Call InitializeAsync first.");
                return false;
            }
            
            try
            {
                string saveFolderGuid = await FindSaveFolderBySlotNameAsync(slotName);
                if (string.IsNullOrEmpty(saveFolderGuid))
                {
                    Debug.LogWarning($"Save slot not found: {slotName}");
                    return false;
                }
                
                string saveFolderPath = GetSaveFolderPath(saveFolderGuid);
                
                if (Directory.Exists(saveFolderPath))
                {
                    Directory.Delete(saveFolderPath, true);
                    Debug.Log($"Save folder deleted: {saveFolderPath}");
                    return true;
                }
                
                Debug.LogWarning($"Save folder not found: {saveFolderPath}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save folder: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get all available save slot GUIDs
        /// </summary>
        public async Task<string[]> GetSaveSlotsAsync()
        {
            if (!_isInitialized)
            {
                return new string[0];
            }
            
            try
            {
                if (!Directory.Exists(_saveDirectoryPath))
                {
                    return new string[0];
                }
                
                string[] folders = Directory.GetDirectories(_saveDirectoryPath);
                List<string> guids = new List<string>();
                
                foreach (string folder in folders)
                {
                    string folderName = Path.GetFileName(folder);
                    // Check if it's a valid GUID folder (has details.json)
                    string detailsPath = Path.Combine(folder, DetailsFileName);
                    if (File.Exists(detailsPath))
                    {
                        guids.Add(folderName);
                    }
                }
                
                return guids.ToArray();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get save slots: {ex.Message}");
                return new string[0];
            }
        }
        
        /// <summary>
        /// Get all save slot details (lightweight info for load menu)
        /// </summary>
        public async Task<List<PvSaveDetails>> GetAllSaveDetailsAsync()
        {
            if (!_isInitialized)
            {
                return new List<PvSaveDetails>();
            }
            
            try
            {
                if (!Directory.Exists(_saveDirectoryPath))
                {
                    return new List<PvSaveDetails>();
                }
                
                string[] folders = Directory.GetDirectories(_saveDirectoryPath);
                List<PvSaveDetails> detailsList = new List<PvSaveDetails>();
                
                foreach (string folder in folders)
                {
                    string folderName = Path.GetFileName(folder);
                    PvSaveDetails details = await LoadDetailsAsync(folderName);
                    if (details != null)
                    {
                        detailsList.Add(details);
                    }
                }
                
                // Sort by save time (newest first)
                detailsList = detailsList.OrderByDescending(d => d.SaveTime).ToList();
                
                return detailsList;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get all save details: {ex.Message}");
                return new List<PvSaveDetails>();
            }
        }
        
        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Cleanup()
        {
            _isInitialized = false;
        }
        
        /// <summary>
        /// Get save folder path by GUID
        /// </summary>
        private string GetSaveFolderPath(string saveFolderGuid)
        {
            return Path.Combine(_saveDirectoryPath, saveFolderGuid);
        }
        
        /// <summary>
        /// Get details.json file path
        /// </summary>
        private string GetDetailsFilePath(string saveFolderGuid)
        {
            return Path.Combine(GetSaveFolderPath(saveFolderGuid), DetailsFileName);
        }
        
        /// <summary>
        /// Get saveData.json file path
        /// </summary>
        private string GetSaveDataFilePath(string saveFolderGuid)
        {
            return Path.Combine(GetSaveFolderPath(saveFolderGuid), SaveDataFileName);
        }
        
        /// <summary>
        /// Load save details from a GUID folder
        /// </summary>
        private async Task<PvSaveDetails> LoadDetailsAsync(string saveFolderGuid)
        {
            try
            {
                string detailsPath = GetDetailsFilePath(saveFolderGuid);
                if (!File.Exists(detailsPath))
                {
                    return null;
                }
                
                string json = await File.ReadAllTextAsync(detailsPath);
                PvSaveDetails details = JsonUtility.FromJson<PvSaveDetails>(json);
                if (details != null)
                {
                    details.SaveFolderGuid = saveFolderGuid;
                }
                return details;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load save details: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Find save folder GUID by slot name
        /// </summary>
        private async Task<string> FindSaveFolderBySlotNameAsync(string slotName)
        {
            try
            {
                if (!Directory.Exists(_saveDirectoryPath))
                {
                    return null;
                }
                
                string[] folders = Directory.GetDirectories(_saveDirectoryPath);
                
                foreach (string folder in folders)
                {
                    string folderName = Path.GetFileName(folder);
                    PvSaveDetails details = await LoadDetailsAsync(folderName);
                    if (details != null && details.SaveSlotName == slotName)
                    {
                        return folderName;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to find save folder by slot name: {ex.Message}");
                return null;
            }
        }
    }
}

