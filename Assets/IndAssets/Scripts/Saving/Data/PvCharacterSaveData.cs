using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Saving.Data
{
    /// <summary>
    /// Extended character save data that includes equipment instance IDs
    /// Extends PvCharacterEquipmentData with instance tracking
    /// </summary>
    [Serializable]
    public class PvCharacterSaveData
    {
        [SerializeField] private string characterName;
        [SerializeField] private string characterId; // Unique ID for the character
        
        // Store instance IDs instead of just names
        [SerializeField] private List<string> weaponInstanceIds = new List<string>(); // Max 2 weapons
        [SerializeField] private List<string> relicInstanceIds = new List<string>(); // Max 3 relics
        
        public string CharacterName
        {
            get => characterName;
            set => characterName = value;
        }
        
        public string CharacterId
        {
            get => characterId;
            set => characterId = value;
        }
        
        public List<string> WeaponInstanceIds => weaponInstanceIds;
        public List<string> RelicInstanceIds => relicInstanceIds;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public PvCharacterSaveData(PvSoBattleUnitData unitData)
        {
            characterName = unitData.m_UnitName;
            characterId = unitData.EntryId;
            weaponInstanceIds = new List<string>();
            relicInstanceIds = new List<string>();
        }
        
        /// <summary>
        /// Constructor with character name
        /// </summary>
        public PvCharacterSaveData(string name)
        {
            characterName = name;
            characterId = Guid.NewGuid().ToString();
            weaponInstanceIds = new List<string>();
            relicInstanceIds = new List<string>();
        }
        
        /// <summary>
        /// Set weapon instance ID at specific index (0 or 1)
        /// </summary>
        public void SetWeaponInstanceId(int index, string instanceId)
        {
            if (index < 0 || index >= 2)
            {
                Debug.LogWarning($"Weapon index {index} is out of range. Must be 0 or 1.");
                return;
            }
            
            while (weaponInstanceIds.Count <= index)
            {
                weaponInstanceIds.Add(string.Empty);
            }
            
            weaponInstanceIds[index] = instanceId;
        }
        
        /// <summary>
        /// Set relic instance ID at specific index (0, 1, or 2)
        /// </summary>
        public void SetRelicInstanceId(int index, string instanceId)
        {
            if (index < 0 || index >= 3)
            {
                Debug.LogWarning($"Relic index {index} is out of range. Must be 0, 1, or 2.");
                return;
            }
            
            while (relicInstanceIds.Count <= index)
            {
                relicInstanceIds.Add(string.Empty);
            }
            
            relicInstanceIds[index] = instanceId;
        }
        
        /// <summary>
        /// Clear all equipment
        /// </summary>
        public void ClearEquipment()
        {
            weaponInstanceIds.Clear();
            relicInstanceIds.Clear();
        }
    }
}

