using System;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Saving.Data
{
    /// <summary>
    /// Relic instance data for save system
    /// Each instance represents a unique relic that can be equipped
    /// </summary>
    [Serializable]
    public class PvRelicInstance
    {
        [SerializeField] private string instanceId; // Unique GUID for this instance
        [SerializeField] private string relicDataName; // Reference to PvSoPassiveRelic.PassiveName
        [SerializeField] private bool isEquipped; // Whether this instance is currently equipped
        [SerializeField] private string equippedToCharacterName; // Which character has this equipped (empty if not equipped)
        
        public string InstanceId
        {
            get => instanceId;
            set => instanceId = value;
        }
        
        public string RelicDataName
        {
            get => relicDataName;
            set => relicDataName = value;
        }
        
        public bool IsEquipped => isEquipped;
        
        public string EquippedToCharacterName => equippedToCharacterName;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public PvRelicInstance()
        {
            instanceId = Guid.NewGuid().ToString();
            relicDataName = string.Empty;
            isEquipped = false;
            equippedToCharacterName = string.Empty;
        }
        
        /// <summary>
        /// Constructor with relic data name
        /// </summary>
        public PvRelicInstance(string relicDataName)
        {
            instanceId = Guid.NewGuid().ToString();
            this.relicDataName = relicDataName;
            isEquipped = false;
            equippedToCharacterName = string.Empty;
        }
        
        /// <summary>
        /// Equip this relic instance to a character
        /// </summary>
        public bool EquipTo(string characterName)
        {
            if (isEquipped)
            {
                Debug.LogWarning($"Relic instance {instanceId} is already equipped to {equippedToCharacterName}");
                return false;
            }
            
            isEquipped = true;
            equippedToCharacterName = characterName;
            return true;
        }
        
        /// <summary>
        /// Unequip this relic instance
        /// </summary>
        public bool Unequip()
        {
            if (!isEquipped)
            {
                Debug.LogWarning($"Relic instance {instanceId} is not equipped");
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

