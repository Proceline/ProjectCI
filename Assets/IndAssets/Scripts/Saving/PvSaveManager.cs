using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IndAssets.Scripts.Managers;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Saving.Data;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;
using ProjectCI.CoreSystem.Runtime.Saving.Implementations;
using IndAssets.Scripts.Weapons;
using IndAssets.Scripts.Passives.Relics;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;

namespace ProjectCI.CoreSystem.Runtime.Saving
{
    /// <summary>
    /// Main save/load manager for the game
    /// Handles saving and loading game progress, character equipment, and equipment instances
    /// </summary>
    public class PvSaveManager : MonoBehaviour
    {
        private static PvSaveManager _instance;
        public static PvSaveManager Instance => _instance;
        
        [Header("Save System Configuration")]
        [SerializeField] private bool useCloudSave = false; // Toggle between local and cloud storage
        [SerializeField] private bool enableSaveEncryption = true; // Enable encryption for save files
        
        private IPvSaveSystem _saveSystem;
        private PvSaveData _currentSaveData;
        private PvSaveDetails _currentSaveDetails;

        private bool _isInitialized;

        [SerializeField] private PvSoWeaponAndRelicCollection equipmentsCollection;
        // Equipment data references (for resolving instance names to data)
        private readonly Dictionary<string, PvSoWeaponData> _weaponDataDict = new Dictionary<string, PvSoWeaponData>();
        private readonly Dictionary<string, PvSoPassiveRelic> _relicDataDict = new Dictionary<string, PvSoPassiveRelic>();
        
        public bool IsInitialized => _isInitialized;

        public PvSaveData CurrentSaveData 
        {
            get 
            {
                if (_currentSaveData == null) throw new Exception("Save data is not initialized");
                return _currentSaveData;
            }
        }

        public static event Action<List<PvSaveDetails>> onAllSaveDataListed;

        private void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            SetEquipmentDataReferences();
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
                var localSaveSystem = new PvLocalSaveSystem();
                localSaveSystem.EnableEncryption = enableSaveEncryption;
                _saveSystem = localSaveSystem;
            }
            
            bool success = await _saveSystem.InitializeAsync();
            if (success)
            {
                _isInitialized = true;
                Debug.Log("Save system initialized successfully");
                // Initialize save data list after system is ready
                await InitializeSaveDataList();
            }
            else
            {
                Debug.LogError("Failed to initialize save system");
            }
        }

        /// <summary>
        /// Initialize and load all save slot details for display in load menu
        /// Can be called manually to refresh the list
        /// </summary>
        public async Awaitable InitializeSaveDataList()
        {
            if (!_isInitialized || _saveSystem == null)
            {
                Debug.LogError("Save system not initialized");
                return;
            }
            
            var saveDetailsList = await _saveSystem.GetAllSaveDetailsAsync();
            onAllSaveDataListed?.Invoke(saveDetailsList);
            Debug.Log($"Loaded {saveDetailsList.Count} save slot(s)");
        }
        
        /// <summary>
        /// Set equipment data dictionaries for resolving instance names
        /// </summary>
        private void SetEquipmentDataReferences()
        {
            if (!equipmentsCollection)
            {
                return;
            }
            
            foreach (var weaponData in equipmentsCollection.Weapons)
            {
                _weaponDataDict.Add(weaponData.EntryId, weaponData);
            }

            foreach (var relicData in equipmentsCollection.Relics)
            {
                _relicDataDict.Add(relicData.EntryId, relicData);
            }
        }
        
        /// <summary>
        /// Load game data from save file by GUID (for load menu)
        /// </summary>
        public async Task<bool> LoadGameByGuidAsync(PvSaveDetails saveDetails)
        {
            if (!_isInitialized || _saveSystem == null)
            {
                Debug.LogError("Save system not initialized");
                return false;
            }
            
            if (string.IsNullOrEmpty(saveDetails.SaveFolderGuid))
            {
                Debug.LogError("Save folder GUID is null or empty");
                return false;
            }
            
            _currentSaveData = await _saveSystem.LoadByGuidAsync(saveDetails.SaveFolderGuid);
            _currentSaveDetails = saveDetails;
            
            if (_currentSaveData == null)
            {
                Debug.LogError("Failed to load save data");
                return false;
            }
            
            Debug.Log("Game data loaded successfully");
            return true;
        }

        public async void SaveCurrentGame()
        {
            if (_currentSaveData == null || _currentSaveDetails == null)
            {
                Debug.LogError("No save data to save");
                return;
            }

            await SaveGameAsync(_currentSaveDetails.SaveSlotName);
        }

        public async Awaitable CreateNewGameAsync(string slotName)
        {
            if (!_isInitialized || _saveSystem == null)
            {
                Debug.LogError("Save system not initialized");
                return;
            }

            if (string.IsNullOrEmpty(slotName))
            {
                Debug.LogError("Save slot name is null or empty");
                return;
            }
            
            _currentSaveData = new PvSaveData();
            await SaveGameAsync(slotName);
        }
        
        /// <summary>
        /// Save game data to file
        /// </summary>
        public async Task<bool> SaveGameAsync(string slotName)
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
            
            if (string.IsNullOrEmpty(slotName))
            {
                Debug.LogError("Save Data Name cannot be Empty!");
                return false;
            }
            
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
        
        #region Character Management
        
        /// <summary>
        /// Unlock a character
        /// </summary>
        public void UnlockCharacter(PvSoBattleUnitData unitData)
        {
            CurrentSaveData.AddUnlockedCharacter(unitData.EntryId);
            var newCharData = new PvCharacterSaveData(unitData);
            SetCharacterEquipmentData(newCharData);
        }
        
        /// <summary>
        /// Check if character is unlocked
        /// </summary>
        public bool IsCharacterUnlocked(string characterId)
        {
            return _currentSaveData != null && _currentSaveData.IsCharacterUnlocked(characterId);
        }
        
        /// <summary>
        /// Set character equipment data
        /// </summary>
        public void SetCharacterEquipmentData(PvCharacterSaveData characterData)
        {
            CurrentSaveData.SetCharacterData(characterData);
        }
        
        /// <summary>
        /// Get character equipment data by name
        /// </summary>
        public PvCharacterSaveData GetCharacterEquipmentData(string characterName)
        {
            return _currentSaveData?.GetCharacterDataByName(characterName);
        }
        
        #endregion
        
        #region Equipment Instance Management
        
        /// <summary>
        /// Add a weapon instance to inventory
        /// </summary>
        public PvWeaponInstance AddWeaponInstance(string weaponDataName)
        {
            if (!_weaponDataDict.TryGetValue(weaponDataName, out var weaponData)) return null;
            var instance = new PvWeaponInstance(weaponData);
            CurrentSaveData.AddWeaponInstance(instance);
            Debug.Log($"Added weapon instance: {instance.InstanceId} ({weaponDataName})");
            
            return instance;

        }
        
        public void AddWeaponInstance(PvSoWeaponData weaponData)
        {
            var instance = new PvWeaponInstance(weaponData);
            CurrentSaveData.AddWeaponInstance(instance);
            Debug.Log($"Added weapon instance: {instance.InstanceId} ({weaponData.name})");
        }
        
        /// <summary>
        /// Add a relic instance to inventory
        /// </summary>
        public PvRelicInstance AddRelicInstance(string relicDataName)
        {
            if (!_relicDataDict.TryGetValue(relicDataName, out var relicData)) return null;
            var instance = new PvRelicInstance(relicData);
            CurrentSaveData.AddRelicInstance(instance);
            Debug.Log($"Added relic instance: {instance.InstanceId} ({relicDataName})");
            return instance;
        }
        
        public void AddRelicInstance(PvSoPassiveRelic relicData)
        {
            var instance = new PvRelicInstance(relicData);
            CurrentSaveData.AddRelicInstance(instance);
            Debug.Log($"Added relic instance: {instance.InstanceId} ({relicData.name})");
        }

        public List<PvCharacterSaveData> GetUnlockedCharacters() => CurrentSaveData.CharacterEquipmentData;
        
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
        
        private void OnDestroy()
        {
            _saveSystem?.Cleanup();
        }
    }
}

