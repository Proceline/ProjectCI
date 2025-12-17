using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI
{
    /// <summary>
    /// Custom dropdown component for equipment selection with tooltip support
    /// </summary>
    public class PvMnEquipmentDropdown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private bool isWeaponDropdown;
        
        private const string EMPTY_OPTION_TEXT = "(Empty)";
        
        // Store instanceIds for each option (index 0 is empty, so instanceIds[0] is empty string)
        private List<string> _instanceIds = new List<string>();
        // Store display names for dropdown
        private List<string> _displayNames = new List<string>();
        private string _currentCharacterId; // Current character ID for this dropdown
        private int _slotIndex = -1; // Slot index for this dropdown (0, 1 for weapons; 0, 1, 2 for relics)
        [NonSerialized] private bool _isDropdownOpen = false; // Track if dropdown is currently open
        [NonSerialized] private int _closedChildrenCount;

        [SerializeField] private UnityEvent<string, string, int> onEquipmentEquipped;
        [SerializeField] private UnityEvent<string> onSlotHovered;
        [SerializeField] private PvSoEquipDataUpdateEvent onEquipDataUpdateEvent;

        private void Awake()
        {
            if (dropdown)
            {
                dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
                _closedChildrenCount = dropdown.transform.childCount;
            }
        }

        void OnEnable()
        {
            onEquipDataUpdateEvent.RegisterCallback(RefreshWithEquippedStatus);
        }

        void OnDisable()
        {
            onEquipDataUpdateEvent.UnregisterCallback(RefreshWithEquippedStatus);
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
        public void Initialize(List<string> instanceIds, List<string> displayNames, string currentCharacterId, int slotIndex = -1)
        {
            _instanceIds = instanceIds ?? new List<string>();
            _displayNames = displayNames ?? new List<string>();
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
            if (!dropdown) return;

            string instanceId = GetValue();
            if (string.IsNullOrEmpty(instanceId)) return;

            onSlotHovered?.Invoke(instanceId);
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            // Empty
        }
        
        /// <summary>
        /// Call this when dropdown option is hovered (by dropdown item index)
        /// </summary>
        public void OnOptionHoveredByIndex(int dropdownItemIndex)
        {
            // dropdownItemIndex: 0 = empty, 1+ = actual equipment
            if (dropdownItemIndex == 0)
            {
                return;
            }
            
            // Convert dropdown index to instanceIds index (subtract 1 because index 0 is empty)
            int instanceIndex = dropdownItemIndex - 1;
            if (instanceIndex >= 0 && instanceIndex < _instanceIds.Count)
            {
                string instanceId = _instanceIds[instanceIndex];
                onSlotHovered?.Invoke(instanceId);
            }
        }
        
        /// <summary>
        /// Update method to monitor dropdown state and setup hover events when opened
        /// </summary>
        private void Update()
        {
            if (!dropdown) return;
            
            bool isOpen = dropdown.transform.childCount > _closedChildrenCount;
            
            // If dropdown just opened, setup hover events
            if (isOpen && !_isDropdownOpen)
            {
                SetupDropdownItemHoverEvents();
                _isDropdownOpen = true;
            }
            // If dropdown just closed, reset state
            else if (!isOpen && _isDropdownOpen)
            {
                _isDropdownOpen = false;
                //TODO: Hide tooltip when dropdown closes
            }
        }
        
        /// <summary>
        /// Refresh dropdown with new instanceIds, displayNames and equipped status
        /// </summary>
        /// <param name="instanceIds">List of instance IDs</param>
        /// <param name="displayNames">List of display names corresponding to instanceIds</param>
        /// <param name="equippedInstanceIds">Dictionary of instance IDs that are currently equipped and the character they are equipped to</param>
        private void RefreshWithEquippedStatus(List<string> instanceIds, List<string> displayNames, Dictionary<string, string> equippedInstanceIds)
        {
            if (!dropdown) return;
            
            // Save current selected value before refresh
            string currentSelectedInstanceId = GetValue();
            
            // Ensure both lists have the same count
            if (instanceIds.Count != displayNames.Count)
            {
                Debug.LogError($"InstanceIds count ({instanceIds.Count}) doesn't match DisplayNames count ({displayNames.Count})");
                return;
            }
            
            // Update internal lists
            _instanceIds = instanceIds;
            _displayNames = displayNames;
            
            // Update display names to show equipped status
            for (int i = 0; i < _displayNames.Count; i++)
            {
                string instanceId = _instanceIds[i];
                if (!string.IsNullOrEmpty(instanceId) && equippedInstanceIds.TryGetValue(instanceId, out string equippedToCharacterId))
                {
                    // Check if equipped to current character or other character
                    bool isEquippedToCurrentCharacter = equippedToCharacterId == _currentCharacterId;
                    
                    // Add equipped status marker to display name
                    if (isEquippedToCurrentCharacter)
                    {
                        _displayNames[i] = $"{_displayNames[i]} [已装备]";
                    }
                    else
                    {
                        _displayNames[i] = $"{_displayNames[i]} [已被装备]";
                    }
                }
            }
            
            // Update dropdown options
            dropdown.ClearOptions();
            // Add empty option at the beginning (index 0)
            dropdown.AddOptions(new List<string> { EMPTY_OPTION_TEXT });
            // Add available equipment options with updated display names
            dropdown.AddOptions(_displayNames);
            
            // Restore previous selection
            if (!string.IsNullOrEmpty(currentSelectedInstanceId))
            {
                SetValueWithoutNotify(currentSelectedInstanceId);
            }
        }
        
        /// <summary>
        /// Setup hover events for dropdown items
        /// </summary>
        private void SetupDropdownItemHoverEvents()
        {
            if (!dropdown) return;
            
            // Find the content area where items are located
            // Structure: template -> Viewport -> Content -> Items
            Transform viewport = dropdown.transform.GetChild(_closedChildrenCount).Find("Viewport");
            if (viewport == null) return;
            
            Transform content = viewport.Find("Content");
            if (content == null) return;
            
            // Get all item toggles
            Toggle[] itemToggles = content.GetComponentsInChildren<Toggle>();
            
            for (int i = 0; i < itemToggles.Length; i++)
            {
                int itemIndex = i; // Capture index for closure
                Toggle itemToggle = itemToggles[i];
                
                // Get or add EventTrigger component
                EventTrigger trigger = itemToggle.gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                {
                    trigger = itemToggle.gameObject.AddComponent<EventTrigger>();
                }
                
                // Remove existing PointerEnter entry if any
                trigger.triggers.RemoveAll(entry => entry.eventID == EventTriggerType.PointerEnter);
                trigger.triggers.RemoveAll(entry => entry.eventID == EventTriggerType.PointerExit);
                
                // Add PointerEnter event
                EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
                pointerEnter.eventID = EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((eventData) => { OnOptionHoveredByIndex(itemIndex); });
                trigger.triggers.Add(pointerEnter);
                
                // Add PointerExit event to hide tooltip
                EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                pointerExit.eventID = EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((eventData) => 
                { 
                    //TODO: Hide tooltip when dropdown closes
                });
                trigger.triggers.Add(pointerExit);
            }
        }
    }
}

