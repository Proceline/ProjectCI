using System;
using IndAssets.Scripts.Weapons;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Saving.Data
{
    /// <summary>
    /// Weapon instance data for save system
    /// Each instance represents a unique weapon that can be equipped
    /// </summary>
    [Serializable]
    public class PvWeaponInstance
    {
        [SerializeField] private string instanceId; // Unique GUID for this instance
        [SerializeField] private string weaponDataId; // Reference to PvSoWeaponData.weaponName
        [SerializeField] private bool isEquipped; // Whether this instance is currently equipped
        [SerializeField] private string equippedToCharacterName; // Which character has this equipped (empty if not equipped)
        
        public string InstanceId
        {
            get => instanceId;
            set => instanceId = value;
        }
        
        public string WeaponDataId
        {
            get => weaponDataId;
            set => weaponDataId = value;
        }
        
        public bool IsEquipped => isEquipped;
        
        public string EquippedToCharacterName => equippedToCharacterName;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public PvWeaponInstance(PvSoWeaponData originData)
        {
            instanceId = Guid.NewGuid().ToString();
            weaponDataId = originData.EntryId;
            isEquipped = false;
            equippedToCharacterName = string.Empty;
        }
        
        /// <summary>
        /// Equip this weapon instance to a character
        /// </summary>
        public bool EquipTo(string characterName)
        {
            if (isEquipped)
            {
                Debug.LogWarning($"Weapon instance {instanceId} is already equipped to {equippedToCharacterName}");
                return false;
            }
            
            isEquipped = true;
            equippedToCharacterName = characterName;
            return true;
        }
        
        /// <summary>
        /// Unequip this weapon instance
        /// </summary>
        public bool Unequip()
        {
            if (!isEquipped)
            {
                Debug.LogWarning($"Weapon instance {instanceId} is not equipped");
                return false;
            }
            
            isEquipped = false;
            equippedToCharacterName = string.Empty;
            return true;
        }
        
        /// <summary>
        /// Check if this instance is available (not equipped)
        /// </summary>
        public bool IsAvailable()
        {
            return !isEquipped;
        }
    }
}

