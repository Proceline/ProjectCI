using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Saving.Data;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.Saving.Implementations
{
    /// <summary>
    /// Local file-based save system using JSON format
    /// Stores saves in Application.persistentDataPath
    /// </summary>
    public class PvLocalSaveSystem : IPvSaveSystem
    {
        private const string SaveDirectoryName = "Saves";
        private const string SaveFileExtension = ".json";
        
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
        /// Save game data to local file
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
                string filePath = GetSaveFilePath(slotName);
                string json = JsonUtility.ToJson(saveData, true);
                
                // Write to temporary file first, then rename (atomic operation)
                string tempFilePath = filePath + ".tmp";
                await File.WriteAllTextAsync(tempFilePath, json);
                
                // Replace original file with temp file
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempFilePath, filePath);
                
                Debug.Log($"Game data saved to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save game data: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Load game data from local file
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
                string filePath = GetSaveFilePath(slotName);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"Save file not found: {filePath}");
                    return null;
                }
                
                string json = await File.ReadAllTextAsync(filePath);
                PvSaveData saveData = JsonUtility.FromJson<PvSaveData>(json);
                
                if (saveData == null)
                {
                    Debug.LogError("Failed to deserialize save data");
                    return null;
                }
                
                Debug.Log($"Game data loaded from: {filePath}");
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
                string filePath = GetSaveFilePath(slotName);
                return File.Exists(filePath);
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Delete a save slot
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
                string filePath = GetSaveFilePath(slotName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"Save file deleted: {filePath}");
                    return true;
                }
                
                Debug.LogWarning($"Save file not found: {filePath}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save file: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get all available save slot names
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
                
                string[] files = Directory.GetFiles(_saveDirectoryPath, $"*{SaveFileExtension}");
                string[] slotNames = new string[files.Length];
                
                for (int i = 0; i < files.Length; i++)
                {
                    string fileName = Path.GetFileNameWithoutExtension(files[i]);
                    slotNames[i] = fileName;
                }
                
                return slotNames;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get save slots: {ex.Message}");
                return new string[0];
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
        /// Get full file path for a save slot
        /// </summary>
        private string GetSaveFilePath(string slotName)
        {
            string fileName = $"{slotName}{SaveFileExtension}";
            return Path.Combine(_saveDirectoryPath, fileName);
        }
    }
}

