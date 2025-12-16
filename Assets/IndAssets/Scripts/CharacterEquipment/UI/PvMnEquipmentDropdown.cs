using System.Collections.Generic;
using IndAssets.Scripts.Passives.Relics;
using IndAssets.Scripts.Weapons;
using ProjectCI.CoreSystem.Runtime.Saving;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI
{
    /// <summary>
    /// Custom dropdown component for equipment selection with tooltip support
    /// </summary>
    public class PvMnEquipmentDropdown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private bool isWeaponDropdown; // true for weapon, false for relic
        [SerializeField] private PvMnEquipmentTooltip tooltip;
        
        private const string EMPTY_OPTION_TEXT = "(Empty)";
        
        // Store instanceIds for each option (index 0 is empty, so instanceIds[0] is empty string)
        private List<string> _instanceIds = new List<string>();
        // Store display names for dropdown
        private List<string> _displayNames = new List<string>();
        private Dictionary<string, PvSoWeaponData> _weaponInfoDict = new Dictionary<string, PvSoWeaponData>();
        private Dictionary<string, PvSoPassiveRelic> _relicInfoDict = new Dictionary<string, PvSoPassiveRelic>();
        private string _currentHoveredOption;
        private string _currentCharacterId; // Current character ID for this dropdown
        private int _slotIndex = -1; // Slot index for this dropdown (0, 1 for weapons; 0, 1, 2 for relics)

        [SerializeField] private UnityEvent<string, string, int> onEquipmentEquipped;
        
        private void Awake()
        {
            if (dropdown)
            {
                dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
        }
        
        private void OnDestroy()
        {
            if (dropdown)
            {
                dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            }
        }
        
        /// <summary>
        /// Initialize dropdown with instanceIds and display names
        /// </summary>
        public void Initialize(List<string> instanceIds, List<string> displayNames, 
            Dictionary<string, PvSoWeaponData> weaponInfoDict, 
            Dictionary<string, PvSoPassiveRelic> relicInfoDict, string currentCharacterId, int slotIndex = -1)
        {
            _instanceIds = instanceIds ?? new List<string>();
            _displayNames = displayNames ?? new List<string>();
            _weaponInfoDict = weaponInfoDict ?? new Dictionary<string, PvSoWeaponData>();
            _relicInfoDict = relicInfoDict ?? new Dictionary<string, PvSoPassiveRelic>();
            _currentCharacterId = currentCharacterId;
            _slotIndex = slotIndex;
            
            // Ensure both lists have the same count
            if (_instanceIds.Count != _displayNames.Count)
            {
                Debug.LogWarning($"InstanceIds count ({_instanceIds.Count}) doesn't match DisplayNames count ({_displayNames.Count})");
                int minCount = Mathf.Min(_instanceIds.Count, _displayNames.Count);
                if (_instanceIds.Count > minCount) _instanceIds.RemoveRange(minCount, _instanceIds.Count - minCount);
                if (_displayNames.Count > minCount) _displayNames.RemoveRange(minCount, _displayNames.Count - minCount);
            }
            
            if (dropdown != null)
            {
                dropdown.ClearOptions();
                // Add empty option at the beginning (index 0)
                dropdown.AddOptions(new List<string> { EMPTY_OPTION_TEXT });
                // Add available equipment options with display names
                dropdown.AddOptions(_displayNames);
            }
        }
        
        /// <summary>
        /// Set current selected value by instanceId
        /// </summary>
        public void SetValueWithoutNotify(string instanceId)
        {
            if (dropdown == null) return;
            
            // If instanceId is empty or null, set to empty option (index 0)
            if (string.IsNullOrEmpty(instanceId))
            {
                dropdown.SetValueWithoutNotify(0);
                return;
            }
            
            // Find the index in instanceIds list (offset by 1 because index 0 is empty option)
            int index = _instanceIds.IndexOf(instanceId);
            if (index >= 0)
            {
                dropdown.SetValueWithoutNotify(index + 1); // +1 because first option is empty
            }
            else
            {
                // InstanceId not found, set to empty
                dropdown.SetValueWithoutNotify(0);
            }
        }
        
        /// <summary>
        /// Get current selected instanceId
        /// </summary>
        public string GetValue()
        {
            if (dropdown == null || dropdown.value < 0)
            {
                return string.Empty;
            }
            
            // Index 0 is the empty option
            if (dropdown.value == 0)
            {
                return string.Empty;
            }
            
            // Adjust index (subtract 1 because first option is empty)
            int adjustedIndex = dropdown.value - 1;
            if (adjustedIndex >= 0 && adjustedIndex < _instanceIds.Count)
            {
                return _instanceIds[adjustedIndex];
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Enable or disable dropdown interaction
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (dropdown != null)
            {
                dropdown.interactable = interactable;
            }
        }
        
        private void OnDropdownValueChanged(int index)
        {
            if (index == 0)
            {
                Debug.LogWarning("Equipment unequipped");
                return;
            }

            if (string.IsNullOrEmpty(_currentCharacterId))
            {
                Debug.LogError("Character ID is null or empty");
                return;
            }

            onEquipmentEquipped?.Invoke(_instanceIds[index - 1], _currentCharacterId, _slotIndex);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltip == null || dropdown == null) return;
            
            // Get hovered option from dropdown template
            var template = dropdown.template;
            if (template != null && template.gameObject.activeSelf)
            {
                // Try to find which option is being hovered
                // This is a simplified version - you may need to enhance this based on your UI setup
                UpdateTooltipForCurrentOption();
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltip != null)
            {
                tooltip.HideTooltip();
            }
        }
        
        /// <summary>
        /// Update tooltip for currently selected or hovered option
        /// </summary>
        private void UpdateTooltipForCurrentOption()
        {
            if (tooltip == null || dropdown == null) return;
            
            string instanceId = GetValue();
            if (string.IsNullOrEmpty(instanceId)) return;
            
            UpdateTooltipForInstanceId(instanceId);
        }
        
        /// <summary>
        /// Update tooltip for a specific instanceId
        /// </summary>
        private void UpdateTooltipForInstanceId(string instanceId)
        {
            if (tooltip == null || string.IsNullOrEmpty(instanceId)) return;
            
            string name = string.Empty;
            string description = string.Empty;
            
            if (isWeaponDropdown)
            {
                // Get weapon instance from save manager
                if (PvSaveManager.Instance != null && PvSaveManager.Instance.CurrentSaveData != null)
                {
                    var weaponInstance = PvSaveManager.Instance.CurrentSaveData.GetWeaponInstance(instanceId);
                    if (weaponInstance != null)
                    {
                        // Get weapon data using WeaponDataId
                        if (_weaponInfoDict.TryGetValue(weaponInstance.WeaponDataId, out var weaponData))
                        {
                            name = weaponData.weaponName;
                            description = weaponData.description;
                        }
                    }
                }
            }
            else
            {
                // Get relic instance from save manager
                if (PvSaveManager.Instance != null && PvSaveManager.Instance.CurrentSaveData != null)
                {
                    var relicInstance = PvSaveManager.Instance.CurrentSaveData.GetRelicInstance(instanceId);
                    if (relicInstance != null)
                    {
                        // Get relic data using RelicDataId
                        if (_relicInfoDict.TryGetValue(relicInstance.RelicDataId, out var relicData))
                        {
                            name = relicData.PassiveName;
                            description = relicData.description;
                        }
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(name))
            {
                tooltip.ShowTooltip(name, description, Input.mousePosition);
            }
        }
        
        /// <summary>
        /// Call this when dropdown option is hovered (from dropdown item)
        /// </summary>
        public void OnOptionHovered(string displayName)
        {
            _currentHoveredOption = displayName;
            // Find instanceId by displayName
            int index = _displayNames.IndexOf(displayName);
            if (index >= 0 && index < _instanceIds.Count)
            {
                UpdateTooltipForInstanceId(_instanceIds[index]);
            }
        }
    }
}

