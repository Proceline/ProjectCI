using System.Collections.Generic;
using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.DependencyInjection;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.Saving.Data;
using TMPro;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI
{
    /// <summary>
    /// Panel for displaying and editing character equipment information
    /// Supports both editable and read-only modes
    /// </summary>
    [StaticInjectableTarget]
    public class PvMnCharacterEquipmentPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelObject;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private PvMnEquipmentDropdown weaponDropdown1;
        [SerializeField] private PvMnEquipmentDropdown weaponDropdown2;
        [SerializeField] private PvMnEquipmentDropdown relicDropdown1;
        [SerializeField] private PvMnEquipmentDropdown relicDropdown2;
        [SerializeField] private PvMnEquipmentDropdown relicDropdown3;
        [SerializeField] private Button closeButton;
        
        [Header("Data")]
        private PvCharacterSaveData _characterData;

        [Inject] private static PvSoWeaponAndRelicCollection _equipmentsCollection;
        
        private bool _isEditable = true;
        private bool _isInBattle = false;
        
        // Available options for dropdowns - store instanceIds and display names separately
        private List<string> _availableWeaponInstanceIds;
        private List<string> _availableWeaponDisplayNames;
        private List<string> _availableRelicInstanceIds;
        private List<string> _availableRelicDisplayNames;
        
        private System.Action _onPanelClosed;
        private System.Action _onDataChanged;
        
        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(ClosePanel);
            }
            
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(ClosePanel);
            }
        }
        
        /// <summary>
        /// Initialize panel with character data and available options
        /// </summary>
        public void Initialize(PvCharacterSaveData characterData, 
            List<string> weaponInstanceIds, List<string> weaponDisplayNames,
            List<string> relicInstanceIds, List<string> relicDisplayNames,
            System.Action onPanelClosed = null,
            System.Action onDataChanged = null)
        {
            _characterData = characterData;
            _availableWeaponInstanceIds = weaponInstanceIds ?? new List<string>();
            _availableWeaponDisplayNames = weaponDisplayNames ?? new List<string>();
            _availableRelicInstanceIds = relicInstanceIds ?? new List<string>();
            _availableRelicDisplayNames = relicDisplayNames ?? new List<string>();
            
            _onPanelClosed = onPanelClosed;
            _onDataChanged = onDataChanged;
            
            UpdateEditableState();
            RefreshDisplay();
        }
        
        /// <summary>
        /// Set battle state (affects editability)
        /// </summary>
        public void SetBattleState(bool isInBattle)
        {
            _isInBattle = isInBattle;
            UpdateEditableState();
        }
        
        /// <summary>
        /// Set editable state manually
        /// </summary>
        public void SetEditable(bool editable)
        {
            _isEditable = editable;
            UpdateEditableState();
        }
        
        /// <summary>
        /// Show panel
        /// </summary>
        public void ShowPanel()
        {
            if (panelObject != null)
            {
                panelObject.SetActive(true);
            }
            RefreshDisplay();
        }
        
        /// <summary>
        /// Hide panel
        /// </summary>
        public void HidePanel()
        {
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Close panel
        /// </summary>
        public void ClosePanel()
        {
            HidePanel();
            _onPanelClosed?.Invoke();
        }
        
        /// <summary>
        /// Refresh display with current character data
        /// </summary>
        public void RefreshDisplay()
        {
            if (_characterData == null) return;
            
            // Update character name
            if (characterNameText != null)
            {
                characterNameText.text = _characterData.CharacterName;
            }

            void InitializeWeaponDropDown(PvMnEquipmentDropdown dropdown, int slotIndex)
            {
                dropdown.Initialize(_availableWeaponInstanceIds, _availableWeaponDisplayNames,
                    _characterData?.CharacterId, slotIndex);
            }
            
            void InitializeRelicDropDown(PvMnEquipmentDropdown dropdown, int slotIndex)
            {
                dropdown.Initialize(_availableRelicInstanceIds, _availableRelicDisplayNames,
                    _characterData?.CharacterId, slotIndex);
            }

            void InitializeDropdownEquipment(PvMnEquipmentDropdown dropdown, int targetIndex, List<string> holdingIndices)
            {
                if (dropdown == null) return;
                
                // If index exists in list, use that value (could be empty string)
                // If index doesn't exist, set to empty
                if (holdingIndices != null && holdingIndices.Count > targetIndex)
                {
                    dropdown.SetValueWithoutNotify(holdingIndices[targetIndex]);
                }
                else
                {
                    // Index doesn't exist, set to empty
                    dropdown.SetValueWithoutNotify(string.Empty);
                }
            }
            
            // Update weapon dropdowns
            if (weaponDropdown1)
            {
                InitializeWeaponDropDown(weaponDropdown1, 0);
                InitializeDropdownEquipment(weaponDropdown1, 0, _characterData.WeaponInstanceIds);
                weaponDropdown1.SetInteractable(_isEditable && !_isInBattle);
            }

            if (weaponDropdown2)
            {
                InitializeWeaponDropDown(weaponDropdown2, 1);
                InitializeDropdownEquipment(weaponDropdown2, 1, _characterData.WeaponInstanceIds);
                weaponDropdown2.SetInteractable(_isEditable && !_isInBattle);
            }

            // Update relic dropdowns
            if (relicDropdown1)
            {
                InitializeRelicDropDown(relicDropdown1, 0);
                InitializeDropdownEquipment(relicDropdown1, 0, _characterData.RelicInstanceIds);
                relicDropdown1.SetInteractable(_isEditable && !_isInBattle);
            }
            
            if (relicDropdown2)
            {
                InitializeRelicDropDown(relicDropdown2, 1);
                InitializeDropdownEquipment(relicDropdown2, 1, _characterData.RelicInstanceIds);
                relicDropdown2.SetInteractable(_isEditable && !_isInBattle);
            }
            
            if (relicDropdown3)
            {
                InitializeRelicDropDown(relicDropdown3, 2);
                InitializeDropdownEquipment(relicDropdown3, 2, _characterData.RelicInstanceIds);
                relicDropdown3.SetInteractable(_isEditable && !_isInBattle);
            }
        }
        
        /// <summary>
        /// Save current dropdown values to character data
        /// </summary>
        public void SaveToCharacterData()
        {
            if (_characterData == null || !_isEditable || _isInBattle) return;
            
            // Save weapons
            if (weaponDropdown1)
            {
                string weapon1 = weaponDropdown1.GetValue();
                _characterData.SetWeaponInstanceId(0, weapon1);
            }
            
            if (weaponDropdown2)
            {
                string weapon2 = weaponDropdown2.GetValue();
                _characterData.SetWeaponInstanceId(1, weapon2);
            }
            
            // Save relics
            if (relicDropdown1)
            {
                string relic1 = relicDropdown1.GetValue();
                _characterData.SetRelicInstanceId(0, relic1);
            }
            
            if (relicDropdown2)
            {
                string relic2 = relicDropdown2.GetValue();
                _characterData.SetRelicInstanceId(1, relic2);
            }
            
            if (relicDropdown3)
            {
                string relic3 = relicDropdown3.GetValue();
                _characterData.SetRelicInstanceId(2, relic3);
            }
            
            _onDataChanged?.Invoke();
        }
        
        private void UpdateEditableState()
        {
            bool canEdit = _isEditable && !_isInBattle;
            
            if (weaponDropdown1 != null) weaponDropdown1.SetInteractable(canEdit);
            if (weaponDropdown2 != null) weaponDropdown2.SetInteractable(canEdit);
            if (relicDropdown1 != null) relicDropdown1.SetInteractable(canEdit);
            if (relicDropdown2 != null) relicDropdown2.SetInteractable(canEdit);
            if (relicDropdown3 != null) relicDropdown3.SetInteractable(canEdit);
        }
    }
}

