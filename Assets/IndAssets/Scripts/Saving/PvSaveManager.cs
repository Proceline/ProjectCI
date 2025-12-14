using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Saving.Data;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;
using ProjectCI.CoreSystem.Runtime.Saving.Implementations;
using ProjectCI.CoreSystem.Runtime.CharacterEquipment.Data;
using IndAssets.Scripts.Weapons;
using IndAssets.Scripts.Passives.Relics;

namespace ProjectCI.CoreSystem.Runtime.Saving
{
    /// <summary>
    /// Main save/load manager for the game
    /// Handles saving and loading game progress, character equipment, and equipment instances
    /// </summary>
    public class PvSaveManager : MonoBehaviour
    {
        private static PvSaveManager _instance;
        
        [Header("Save System Configuration")]
        [SerializeField] private bool useCloudSave = false; // Toggle between local and cloud storage
        [SerializeField] private string defaultSaveSlot = "default";
        
        private IPvSaveSystem _saveSystem;
        private PvSaveData _currentSaveData;
        private bool _isInitialized;
        
        // Equipment data references (for resolving instance names to data)
        private Dictionary<string, PvSoWeaponData> _weaponDataDict = new Dictionary<string, PvSoWeaponData>();
        private Dictionary<string, PvSoPassiveRelic> _relicDataDict = new Dictionary<string, PvSoPassiveRelic>();
        
        public bool IsInitialized => _isInitialized;
        public PvSaveData CurrentSaveData => _currentSaveData;
        
        private void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            InitializeSaveSystem();
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// Initialize the save system
        /// </summary>
        private async void InitializeSaveSystem()
        {
            // Create appropriate save system based on configuration
            if (useCloudSave)
            {
                _saveSystem = new PvCloudSaveSystem();
            }
            else
            {
                _saveSystem = new PvLocalSaveSystem();
            }
            
            bool success = await _saveSystem.InitializeAsync();
            if (success)
            {
                _isInitialized = true;
                Debug.Log("Save system initialized successfully");
            }
            else
            {
                Debug.LogError("Failed to initialize save system");
            }
        }
        
        /// <summary>
        /// Set equipment data dictionaries for resolving instance names
        /// </summary>
        public void SetEquipmentDataReferences(
            Dictionary<string, PvSoWeaponData> weaponDataDict,
            Dictionary<string, PvSoPassiveRelic> relicDataDict)
        {
            _weaponDataDict = weaponDataDict ?? new Dictionary<string, PvSoWeaponData>();
            _relicDataDict = relicDataDict ?? new Dictionary<string, PvSoPassiveRelic>();
        }
        
        /// <summary>
        /// Load game data from save file
        /// </summary>
        public async Task<bool> LoadGameAsync(string slotName = null)
        {
            if (!_isInitialized || _saveSystem == null)
            {
                Debug.LogError("Save system not initialized");
                return false;
            }
            
            slotName = slotName ?? defaultSaveSlot;
            
            _currentSaveData = await _saveSystem.LoadAsync(slotName);
            
            if (_currentSaveData == null)
            {
                // Create new save data if no save exists
                _currentSaveData = new PvSaveData();
                Debug.Log("No save file found, created new save data");
            }
            else
            {
                Debug.Log("Game data loaded successfully");
            }
            
            return true;
        }

        public async void SaveDefaultGame()
        {
            await SaveGameAsync(defaultSaveSlot);
        }
        
        /// <summary>
        /// Save game data to file
        /// </summary>
        public async Task<bool> SaveGameAsync(string slotName = null)
        {
            if (!_isInitialized || _saveSystem == null)
            {
                Debug.LogError("Save system not initialized");
                return false;
            }
            
            if (_currentSaveData == null)
            {
                Debug.LogWarning("No save data to save. Creating new save data.");
                _currentSaveData = new PvSaveData();
            }
            
            slotName = slotName ?? defaultSaveSlot;
            
            bool success = await _saveSystem.SaveAsync(_currentSaveData, slotName);
            
            if (success)
            {
                Debug.Log("Game data saved successfully");
            }
            else
            {
                Debug.LogError("Failed to save game data");
            }
            
            return success;
        }
        
        /// <summary>
        /// Create new save data (for new game)
        /// </summary>
        public void CreateNewSaveData()
        {
            _currentSaveData = new PvSaveData();
            Debug.Log("New save data created");
        }
        
        #region Character Management
        
        /// <summary>
        /// Unlock a character
        /// </summary>
        public void UnlockCharacter(string characterId)
        {
            if (_currentSaveData == null)
            {
                CreateNewSaveData();
            }
            
            _currentSaveData.AddUnlockedCharacter(characterId);
        }
        
        /// <summary>
        /// Check if character is unlocked
        /// </summary>
        public bool IsCharacterUnlocked(string characterId)
        {
            if (_currentSaveData == null) return false;
            return _currentSaveData.IsCharacterUnlocked(characterId);
        }
        
        /// <summary>
        /// Get all unlocked character IDs
        /// </summary>
        public List<string> GetUnlockedCharacters()
        {
            if (_currentSaveData == null) return new List<string>();
            return new List<string>(_currentSaveData.UnlockedCharacterIds);
        }
        
        /// <summary>
        /// Set character equipment data
        /// </summary>
        public void SetCharacterEquipmentData(PvCharacterSaveData characterData)
        {
            if (_currentSaveData == null)
            {
                CreateNewSaveData();
            }
            
            _currentSaveData.SetCharacterData(characterData);
        }
        
        /// <summary>
        /// Get character equipment data by name
        /// </summary>
        public PvCharacterSaveData GetCharacterEquipmentData(string characterName)
        {
            if (_currentSaveData == null) return null;
            return _currentSaveData.GetCharacterDataByName(characterName);
        }
        
        #endregion
        
        #region Equipment Instance Management
        
        /// <summary>
        /// Add a weapon instance to inventory
        /// </summary>
        public PvWeaponInstance AddWeaponInstance(string weaponDataName)
        {
            if (_currentSaveData == null)
            {
                CreateNewSaveData();
            }
            
            var instance = new PvWeaponInstance(weaponDataName);
            _currentSaveData.AddWeaponInstance(instance);
            Debug.Log($"Added weapon instance: {instance.InstanceId} ({weaponDataName})");
            return instance;
        }
        
        /// <summary>
        /// Add a relic instance to inventory
        /// </summary>
        public PvRelicInstance AddRelicInstance(string relicDataName)
        {
            if (_currentSaveData == null)
            {
                CreateNewSaveData();
            }
            
            var instance = new PvRelicInstance(relicDataName);
            _currentSaveData.AddRelicInstance(instance);
            Debug.Log($"Added relic instance: {instance.InstanceId} ({relicDataName})");
            return instance;
        }
        
        /// <summary>
        /// Get available weapon instances (not equipped)
        /// </summary>
        public List<PvWeaponInstance> GetAvailableWeaponInstances()
        {
            if (_currentSaveData == null) return new List<PvWeaponInstance>();
            return _currentSaveData.GetAvailableWeaponInstances();
        }
        
        /// <summary>
        /// Get available relic instances (not equipped)
        /// </summary>
        public List<PvRelicInstance> GetAvailableRelicInstances()
        {
            if (_currentSaveData == null) return new List<PvRelicInstance>();
            return _currentSaveData.GetAvailableRelicInstances();
        }
        
        /// <summary>
        /// Get weapon instances by data name
        /// </summary>
        public List<PvWeaponInstance> GetWeaponInstancesByDataName(string weaponDataName)
        {
            if (_currentSaveData == null) return new List<PvWeaponInstance>();
            return _currentSaveData.GetWeaponInstancesByDataName(weaponDataName);
        }
        
        /// <summary>
        /// Get relic instances by data name
        /// </summary>
        public List<PvRelicInstance> GetRelicInstancesByDataName(string relicDataName)
        {
            if (_currentSaveData == null) return new List<PvRelicInstance>();
            return _currentSaveData.GetRelicInstancesByDataName(relicDataName);
        }
        
        /// <summary>
        /// Equip weapon instance to character
        /// </summary>
        public bool EquipWeaponToCharacter(string weaponInstanceId, string characterName, int slotIndex)
        {
            if (_currentSaveData == null) return false;
            
            var instance = _currentSaveData.GetWeaponInstance(weaponInstanceId);
            if (instance == null)
            {
                Debug.LogError($"Weapon instance not found: {weaponInstanceId}");
                return false;
            }
            
            if (!instance.IsAvailable())
            {
                Debug.LogError($"Weapon instance {weaponInstanceId} is already equipped to {instance.EquippedToCharacterName}");
                return false;
            }
            
            // Unequip any existing weapon in this slot
            var characterData = _currentSaveData.GetCharacterDataByName(characterName);
            if (characterData != null && characterData.WeaponInstanceIds.Count > slotIndex)
            {
                string existingInstanceId = characterData.WeaponInstanceIds[slotIndex];
                if (!string.IsNullOrEmpty(existingInstanceId))
                {
                    var existingInstance = _currentSaveData.GetWeaponInstance(existingInstanceId);
                    existingInstance?.Unequip();
                }
            }
            
            // Equip new weapon
            if (instance.EquipTo(characterName))
            {
                if (characterData == null)
                {
                    characterData = new PvCharacterSaveData(characterName);
                    _currentSaveData.SetCharacterData(characterData);
                }
                
                characterData.SetWeaponInstanceId(slotIndex, weaponInstanceId);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Equip relic instance to character
        /// </summary>
        public bool EquipRelicToCharacter(string relicInstanceId, string characterName, int slotIndex)
        {
            if (_currentSaveData == null) return false;
            
            var instance = _currentSaveData.GetRelicInstance(relicInstanceId);
            if (instance == null)
            {
                Debug.LogError($"Relic instance not found: {relicInstanceId}");
                return false;
            }
            
            if (!instance.IsAvailable())
            {
                Debug.LogError($"Relic instance {relicInstanceId} is already equipped to {instance.EquippedToCharacterName}");
                return false;
            }
            
            // Unequip any existing relic in this slot
            var characterData = _currentSaveData.GetCharacterDataByName(characterName);
            if (characterData != null && characterData.RelicInstanceIds.Count > slotIndex)
            {
                string existingInstanceId = characterData.RelicInstanceIds[slotIndex];
                if (!string.IsNullOrEmpty(existingInstanceId))
                {
                    var existingInstance = _currentSaveData.GetRelicInstance(existingInstanceId);
                    existingInstance?.Unequip();
                }
            }
            
            // Equip new relic
            if (instance.EquipTo(characterName))
            {
                if (characterData == null)
                {
                    characterData = new PvCharacterSaveData(characterName);
                    _currentSaveData.SetCharacterData(characterData);
                }
                
                characterData.SetRelicInstanceId(slotIndex, relicInstanceId);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Unequip weapon from character
        /// </summary>
        public bool UnequipWeaponFromCharacter(string characterName, int slotIndex)
        {
            if (_currentSaveData == null) return false;
            
            var characterData = _currentSaveData.GetCharacterDataByName(characterName);
            if (characterData == null || characterData.WeaponInstanceIds.Count <= slotIndex)
            {
                return false;
            }
            
            string instanceId = characterData.WeaponInstanceIds[slotIndex];
            if (string.IsNullOrEmpty(instanceId))
            {
                return false;
            }
            
            var instance = _currentSaveData.GetWeaponInstance(instanceId);
            if (instance != null && instance.Unequip())
            {
                characterData.SetWeaponInstanceId(slotIndex, string.Empty);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Unequip relic from character
        /// </summary>
        public bool UnequipRelicFromCharacter(string characterName, int slotIndex)
        {
            if (_currentSaveData == null) return false;
            
            var characterData = _currentSaveData.GetCharacterDataByName(characterName);
            if (characterData == null || characterData.RelicInstanceIds.Count <= slotIndex)
            {
                return false;
            }
            
            string instanceId = characterData.RelicInstanceIds[slotIndex];
            if (string.IsNullOrEmpty(instanceId))
            {
                return false;
            }
            
            var instance = _currentSaveData.GetRelicInstance(instanceId);
            if (instance != null && instance.Unequip())
            {
                characterData.SetRelicInstanceId(slotIndex, string.Empty);
                return true;
            }
            
            return false;
        }
        
        #endregion
        
        #region Conversion Helpers
        
        /// <summary>
        /// Convert PvCharacterEquipmentData to PvCharacterSaveData
        /// This resolves weapon/relic names to instance IDs
        /// </summary>
        public PvCharacterSaveData ConvertEquipmentDataToSaveData(
            PvCharacterEquipmentData equipmentData,
            Dictionary<string, PvSoWeaponData> weaponDataDict,
            Dictionary<string, PvSoPassiveRelic> relicDataDict)
        {
            if (equipmentData == null) return null;
            
            var saveData = new PvCharacterSaveData(equipmentData.CharacterName);
            
            // Convert weapon names to instance IDs
            for (int i = 0; i < equipmentData.Weapons.Count && i < 2; i++)
            {
                string weaponName = equipmentData.Weapons[i];
                if (string.IsNullOrEmpty(weaponName)) continue;
                
                // Find an available instance of this weapon
                var availableInstances = GetWeaponInstancesByDataName(weaponName)
                    .Where(inst => inst.IsAvailable()).ToList();
                
                if (availableInstances.Count > 0)
                {
                    saveData.SetWeaponInstanceId(i, availableInstances[0].InstanceId);
                }
            }
            
            // Convert relic names to instance IDs
            for (int i = 0; i < equipmentData.Relics.Count && i < 3; i++)
            {
                string relicName = equipmentData.Relics[i];
                if (string.IsNullOrEmpty(relicName)) continue;
                
                // Find an available instance of this relic
                var availableInstances = GetRelicInstancesByDataName(relicName)
                    .Where(inst => inst.IsAvailable()).ToList();
                
                if (availableInstances.Count > 0)
                {
                    saveData.SetRelicInstanceId(i, availableInstances[0].InstanceId);
                }
            }
            
            return saveData;
        }
        
        #endregion
        
        private void OnDestroy()
        {
            _saveSystem?.Cleanup();
        }
    }
}

