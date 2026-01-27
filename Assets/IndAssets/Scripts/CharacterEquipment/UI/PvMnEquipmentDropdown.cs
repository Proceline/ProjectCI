using ProjectCI.Utilities.Runtime.Events;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI
{
    /// <summary>
    /// Custom dropdown component for equipment selection with tooltip support
    /// </summary>
    public class PvMnEquipmentDropdown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Dropdown dropdown;
        
        private const string EMPTY_OPTION_TEXT = "(Empty)";

        // Store reference to instanceIds
        private List<string> _instanceIds;
        // Store display names for dropdown
        private List<string> _displayNames = new();

        private readonly Dictionary<string, string> _usedInstancesWithHolders = new();

        private string _currentCharacterId; // Current character ID for this dropdown
        private int _slotIndex = -1; // Slot index for this dropdown (0, 1 for weapons; 0, 1, 2 for relics)
        [NonSerialized] private bool _isDropdownOpen = false; // Track if dropdown is currently open
        [NonSerialized] private int _closedChildrenCount;

        [SerializeField] private UnityEvent onInitialized;
        [SerializeField] private UnityEvent<string, string, int> onEquipmentEquipped;
        [SerializeField] private UnityEvent<string, string, bool> onSlotHovered;
        [SerializeField] private PvSoEquipDataUpdateEvent onEquipDataUpdateEvent;

        [NonSerialized] private bool _isRegistered;

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
            if (!_isRegistered)
            {
                onEquipDataUpdateEvent.RegisterCallback(RefreshWithEquippedStatus);
                _isRegistered = true;
            }
        }

        void OnDisable()
        {
            if (!_isRegistered)
                return;
            onEquipDataUpdateEvent.UnregisterCallback(RefreshWithEquippedStatus);
            _isRegistered = false;
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
        public void Initialize(string currentCharacterId, int slotIndex = -1)
        {
            if (!_isRegistered)
            {
                onEquipDataUpdateEvent.RegisterCallback(RefreshWithEquippedStatus);
                _isRegistered = true;
            }

            _currentCharacterId = currentCharacterId;
            _slotIndex = slotIndex;
            onInitialized.Invoke();
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
            if (string.IsNullOrEmpty(_currentCharacterId))
            {
                Debug.LogError("Character ID is null or empty");
                return;
            }

            // TODO: do unequipment
            if (index == 0)
            {
                Debug.LogWarning("Equipment unequipped");
                return;
            }

            var selectedInstance = _instanceIds[index - 1];
            if (_usedInstancesWithHolders.ContainsKey(selectedInstance))
            {
                dropdown.SetValueWithoutNotify(0);

                foreach (var instancePair in _usedInstancesWithHolders)
                {
                    var characterId = instancePair.Value;
                    if (characterId == _currentCharacterId)
                    {
                        SetValueWithoutNotify(instancePair.Key);
                    }
                }
            }
            else
            {
                onEquipmentEquipped?.Invoke(_instanceIds[index - 1], _currentCharacterId, _slotIndex);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!dropdown) return;

            string instanceId = GetValue();
            if (string.IsNullOrEmpty(instanceId)) return;

            var characterId = _usedInstancesWithHolders.ContainsKey(instanceId) ? _usedInstancesWithHolders[instanceId] : string.Empty;
            onSlotHovered?.Invoke(instanceId, characterId, true);
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            onSlotHovered?.Invoke(string.Empty, string.Empty, false);
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
                var characterId = _usedInstancesWithHolders.ContainsKey(instanceId) ? _usedInstancesWithHolders[instanceId] : string.Empty;
                onSlotHovered?.Invoke(instanceId, characterId, true);
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
            // Update internal lists
            _instanceIds = instanceIds;
            _displayNames.Clear();
            _displayNames = new List<string>(displayNames);
            _usedInstancesWithHolders.Clear();

            for (int i = 0; i < _displayNames.Count; i++)
            {
                string instanceId = _instanceIds[i];
                if (!string.IsNullOrEmpty(instanceId) && equippedInstanceIds.TryGetValue(instanceId, out string equippedToCharacterId))
                {
                    _usedInstancesWithHolders.Add(instanceId, equippedToCharacterId);
                }
            }

            RefreshWithEquippedStatus();
        }

        private void RefreshWithEquippedStatus()
        {
            if (!dropdown) return;

            // Save current selected value before refresh
            string currentSelectedInstanceId = GetValue();

            // Update dropdown options
            dropdown.ClearOptions();
            // Add empty option at the beginning (index 0)
            dropdown.AddOptions(new List<string> { EMPTY_OPTION_TEXT });
            // Add available equipment options with updated display names
            dropdown.AddOptions(_displayNames);

            // Update display names to show equipped status
            for (int i = 0; i < _displayNames.Count; i++)
            {
                string instanceId = _instanceIds[i];
                var outputString = _usedInstancesWithHolders.TryGetValue(instanceId, out var characterId) && characterId != _currentCharacterId ?
                    $"<color=red>{_displayNames[i]}</color>" : _displayNames[i];
                var targetDropdownItem = dropdown.options[i + 1];

                targetDropdownItem.text = outputString;
            }

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

