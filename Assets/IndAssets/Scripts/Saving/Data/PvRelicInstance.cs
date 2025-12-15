using System;
using IndAssets.Scripts.Passives.Relics;
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
        [SerializeField] private string relicDataId; // Reference to PvSoPassiveRelic.PassiveName
        [SerializeField] private bool isEquipped; // Whether this instance is currently equipped
        [SerializeField] private string equippedToCharacterName; // Which character has this equipped (empty if not equipped)
        
        public string InstanceId
        {
            get => instanceId;
            set => instanceId = value;
        }
        
        public string RelicDataId
        {
            get => relicDataId;
            set => relicDataId = value;
        }
        
        public bool IsEquipped => isEquipped;
        
        public string EquippedToCharacterName => equippedToCharacterName;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public PvRelicInstance(PvSoPassiveRelic relic)
        {
            instanceId = Guid.NewGuid().ToString();
            relicDataId = relic.EntryId;
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

