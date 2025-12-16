using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Saving.Data
{
    /// <summary>
    /// Main save data structure containing all game progress
    /// </summary>
    [Serializable]
    public class PvSaveData
    {
        [Header("Version Info")]
        [SerializeField] private string saveVersion = "1.0.0"; // For future compatibility

        [SerializeField] private List<string> unlockedCharacterIds;
        [SerializeField] private List<PvCharacterSaveData> characterEquipmentData;
        [SerializeField] private List<PvWeaponInstance> weaponInstances;
        [SerializeField] private List<PvRelicInstance> relicInstances;
        
        public string SaveVersion
        {
            get => saveVersion;
            set => saveVersion = value;
        }
        
        public List<string> UnlockedCharacterIds => unlockedCharacterIds;
        public List<PvCharacterSaveData> CharacterEquipmentData => characterEquipmentData;
        public List<PvWeaponInstance> WeaponInstances => weaponInstances;
        public List<PvRelicInstance> RelicInstances => relicInstances;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public PvSaveData()
        {
            saveVersion = "1.0.0";
            unlockedCharacterIds = new List<string>();
            characterEquipmentData = new List<PvCharacterSaveData>();
            weaponInstances = new List<PvWeaponInstance>();
            relicInstances = new List<PvRelicInstance>();
        }
        
        /// <summary>
        /// Add unlocked character ID
        /// </summary>
        public void AddUnlockedCharacter(string characterId)
        {
            if (!unlockedCharacterIds.Contains(characterId))
            {
                unlockedCharacterIds.Add(characterId);
            }
        }
        
        /// <summary>
        /// Remove unlocked character ID
        /// </summary>
        public bool RemoveUnlockedCharacter(string characterId)
        {
            return unlockedCharacterIds.Remove(characterId);
        }
        
        /// <summary>
        /// Check if character is unlocked
        /// </summary>
        public bool IsCharacterUnlocked(string characterId)
        {
            return unlockedCharacterIds.Contains(characterId);
        }
        
        /// <summary>
        /// Get character save data by ID
        /// </summary>
        public PvCharacterSaveData GetCharacterDataById(string characterId)
        {
            return characterEquipmentData.Find(data => data.CharacterId == characterId);
        }
        
        /// <summary>
        /// Add or update character equipment data
        /// </summary>
        public void SetCharacterData(PvCharacterSaveData characterData)
        {
            var existing = GetCharacterDataById(characterData.CharacterId);
            if (existing != null)
            {
                int index = characterEquipmentData.IndexOf(existing);
                characterEquipmentData[index] = characterData;
            }
            else
            {
                characterEquipmentData.Add(characterData);
            }
        }
        
        /// <summary>
        /// Get weapon instance by ID
        /// </summary>
        public PvWeaponInstance GetWeaponInstance(string instanceId)
        {
            return weaponInstances.Find(instance => instance.InstanceId == instanceId);
        }
        
        /// <summary>
        /// Get relic instance by ID
        /// </summary>
        public PvRelicInstance GetRelicInstance(string instanceId)
        {
            return relicInstances.Find(instance => instance.InstanceId == instanceId);
        }
        
        /// <summary>
        /// Add weapon instance
        /// </summary>
        public void AddWeaponInstance(PvWeaponInstance instance)
        {
            if (instance == null) return;
            
            if (GetWeaponInstance(instance.InstanceId) == null)
            {
                weaponInstances.Add(instance);
            }
        }
        
        /// <summary>
        /// Add relic instance
        /// </summary>
        public void AddRelicInstance(PvRelicInstance instance)
        {
            if (instance == null) return;
            
            if (GetRelicInstance(instance.InstanceId) == null)
            {
                relicInstances.Add(instance);
            }
        }
        
        /// <summary>
        /// Get all available (unequipped) weapon instances
        /// </summary>
        public List<PvWeaponInstance> GetAvailableWeaponInstances()
        {
            return weaponInstances.FindAll(instance => instance.IsAvailable());
        }
        
        /// <summary>
        /// Get all available (unequipped) relic instances
        /// </summary>
        public List<PvRelicInstance> GetAvailableRelicInstances()
        {
            return relicInstances.FindAll(instance => instance.IsAvailable());
        }
        
        /// <summary>
        /// Get weapon instances by data name
        /// </summary>
        public List<PvWeaponInstance> GetWeaponInstancesByDataName(string weaponDataId)
        {
            return weaponInstances.FindAll(instance => instance.WeaponDataId == weaponDataId);
        }
        
        /// <summary>
        /// Get relic instances by data name
        /// </summary>
        public List<PvRelicInstance> GetRelicInstancesById(string relicDataId)
        {
            return relicInstances.FindAll(instance => instance.RelicDataId == relicDataId);
        }
    }
}

