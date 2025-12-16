using System.Collections.Generic;
using IndAssets.Scripts.Passives.Relics;
using IndAssets.Scripts.Weapons;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

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
        
        private List<string> _availableOptions = new List<string>();
        private Dictionary<string, PvSoWeaponData> _weaponInfoDict = new Dictionary<string, PvSoWeaponData>();
        private Dictionary<string, PvSoPassiveRelic> _relicInfoDict = new Dictionary<string, PvSoPassiveRelic>();
        private string _currentHoveredOption;
        
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
        /// Initialize dropdown with available options
        /// </summary>
        public void Initialize(List<string> options, Dictionary<string, PvSoWeaponData> weaponInfoDict = null, 
            Dictionary<string, PvSoPassiveRelic> relicInfoDict = null)
        {
            _availableOptions = options ?? new List<string>();
            _weaponInfoDict = weaponInfoDict ?? new Dictionary<string, PvSoWeaponData>();
            _relicInfoDict = relicInfoDict ?? new Dictionary<string, PvSoPassiveRelic>();
            
            if (dropdown != null)
            {
                dropdown.ClearOptions();
                // Add empty option at the beginning
                dropdown.AddOptions(new List<string> { EMPTY_OPTION_TEXT });
                // Add available equipment options
                dropdown.AddOptions(_availableOptions);
            }
        }
        
        /// <summary>
        /// Set current selected value
        /// </summary>
        public void SetValue(string value)
        {
            if (dropdown == null) return;
            
            // If value is empty or null, set to empty option (index 0)
            if (string.IsNullOrEmpty(value))
            {
                dropdown.value = 0;
                return;
            }
            
            // Find the index in available options (offset by 1 because index 0 is empty option)
            int index = _availableOptions.IndexOf(value);
            if (index >= 0)
            {
                dropdown.value = index + 1; // +1 because first option is empty
            }
        }
        
        /// <summary>
        /// Get current selected value
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
            if (adjustedIndex >= 0 && adjustedIndex < _availableOptions.Count)
            {
                return _availableOptions[adjustedIndex];
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
            // Handle value change if needed
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
            
            string optionName = GetValue();
            if (string.IsNullOrEmpty(optionName)) return;
            
            string name = string.Empty;
            string description = string.Empty;
            
            if (isWeaponDropdown && _weaponInfoDict.ContainsKey(optionName))
            {
                var weaponInfo = _weaponInfoDict[optionName];
                name = weaponInfo.weaponName;
                description = weaponInfo.description;
            }
            else if (!isWeaponDropdown && _relicInfoDict.ContainsKey(optionName))
            {
                var relicInfo = _relicInfoDict[optionName];
                name = relicInfo.PassiveName;
                description = relicInfo.description;
            }
            
            if (!string.IsNullOrEmpty(name))
            {
                tooltip.ShowTooltip(name, description, Input.mousePosition);
            }
        }
        
        /// <summary>
        /// Call this when dropdown option is hovered (from dropdown item)
        /// </summary>
        public void OnOptionHovered(string optionName)
        {
            _currentHoveredOption = optionName;
            UpdateTooltipForOption(optionName);
        }
        
        private void UpdateTooltipForOption(string optionName)
        {
            if (tooltip == null || string.IsNullOrEmpty(optionName)) return;
            
            string name = string.Empty;
            string description = string.Empty;
            
            if (isWeaponDropdown && _weaponInfoDict.ContainsKey(optionName))
            {
                var weaponInfo = _weaponInfoDict[optionName];
                name = weaponInfo.weaponName;
                description = weaponInfo.description;
            }
            else if (!isWeaponDropdown && _relicInfoDict.ContainsKey(optionName))
            {
                var relicInfo = _relicInfoDict[optionName];
                name = relicInfo.PassiveName;
                description = relicInfo.description;
            }
            
            if (!string.IsNullOrEmpty(name))
            {
                tooltip.ShowTooltip(name, description, Input.mousePosition);
            }
        }
    }
}

