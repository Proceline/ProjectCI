using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment.Data
{
    /// <summary>
    /// Serializable class for storing character equipment data at runtime
    /// Character can hold up to 2 weapons and 3 relics
    /// This data can be serialized to JSON/Binary for save/load functionality
    /// </summary>
    [Serializable]
    public class PvCharacterEquipmentData
    {
        [Header("Character Info")]
        [SerializeField] private string characterName;
        
        [Header("Equipment")]
        [SerializeField] private List<string> weapons = new List<string>(); // Max 2 weapons
        [SerializeField] private List<string> relics = new List<string>(); // Max 3 relics
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public PvCharacterEquipmentData()
        {
            characterName = string.Empty;
            weapons = new List<string>();
            relics = new List<string>();
        }
        
        /// <summary>
        /// Constructor with character name
        /// </summary>
        public PvCharacterEquipmentData(string name)
        {
            characterName = name;
            weapons = new List<string>();
            relics = new List<string>();
        }
        
        public string CharacterName
        {
            get => characterName;
            set => characterName = value;
        }
        
        public List<string> Weapons => weapons;
        public List<string> Relics => relics;
        
        /// <summary>
        /// Set weapon at specific index (0 or 1)
        /// </summary>
        public void SetWeapon(int index, string weaponName)
        {
            if (index < 0 || index >= 2)
            {
                Debug.LogWarning($"Weapon index {index} is out of range. Must be 0 or 1.");
                return;
            }
            
            while (weapons.Count <= index)
            {
                weapons.Add(string.Empty);
            }
            
            weapons[index] = weaponName;
        }
        
        /// <summary>
        /// Set relic at specific index (0, 1, or 2)
        /// </summary>
        public void SetRelic(int index, string relicName)
        {
            if (index < 0 || index >= 3)
            {
                Debug.LogWarning($"Relic index {index} is out of range. Must be 0, 1, or 2.");
                return;
            }
            
            while (relics.Count <= index)
            {
                relics.Add(string.Empty);
            }
            
            relics[index] = relicName;
        }
        
        /// <summary>
        /// Clear all equipment
        /// </summary>
        public void ClearEquipment()
        {
            weapons.Clear();
            relics.Clear();
        }
    }
}

