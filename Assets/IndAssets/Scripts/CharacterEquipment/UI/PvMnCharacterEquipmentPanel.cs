using System.Collections.Generic;
using IndAssets.Scripts.Passives.Relics;
using IndAssets.Scripts.Weapons;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.CharacterEquipment.Data;
using TMPro;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI
{
    /// <summary>
    /// Panel for displaying and editing character equipment information
    /// Supports both editable and read-only modes
    /// </summary>
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
        private PvCharacterEquipmentData currentCharacterData;
        
        private bool _isEditable = true;
        private bool _isInBattle = false;
        
        // Available options for dropdowns
        private List<string> _availableWeapons = new List<string>();
        private List<string> _availableRelics = new List<string>();
        private Dictionary<string, PvSoWeaponData> _weaponInfoDict;
        private Dictionary<string, PvSoPassiveRelic> _relicInfoDict;
        
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
        public void Initialize(PvCharacterEquipmentData characterData, 
            List<string> availableWeapons, List<string> availableRelics,
            Dictionary<string, PvSoWeaponData> weaponInfoDict = null,
            Dictionary<string, PvSoPassiveRelic> relicInfoDict = null,
            System.Action onPanelClosed = null,
            System.Action onDataChanged = null)
        {
            currentCharacterData = characterData;
            _availableWeapons = availableWeapons ?? new List<string>();
            _availableRelics = availableRelics ?? new List<string>();
            _weaponInfoDict = weaponInfoDict ?? new Dictionary<string, PvSoWeaponData>();
            _relicInfoDict = relicInfoDict ?? new Dictionary<string, PvSoPassiveRelic>();
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
            if (currentCharacterData == null) return;
            
            // Update character name
            if (characterNameText != null)
            {
                characterNameText.text = currentCharacterData.CharacterName;
            }
            
            // Update weapon dropdowns
            if (weaponDropdown1 != null)
            {
                weaponDropdown1.Initialize(_availableWeapons, _weaponInfoDict, _relicInfoDict);
                if (currentCharacterData.Weapons.Count > 0)
                {
                    weaponDropdown1.SetValue(currentCharacterData.Weapons[0]);
                }
                weaponDropdown1.SetInteractable(_isEditable && !_isInBattle);
            }
            
            if (weaponDropdown2 != null)
            {
                weaponDropdown2.Initialize(_availableWeapons, _weaponInfoDict, _relicInfoDict);
                if (currentCharacterData.Weapons.Count > 1)
                {
                    weaponDropdown2.SetValue(currentCharacterData.Weapons[1]);
                }
                weaponDropdown2.SetInteractable(_isEditable && !_isInBattle);
            }
            
            // Update relic dropdowns
            if (relicDropdown1 != null)
            {
                relicDropdown1.Initialize(_availableRelics, _weaponInfoDict, _relicInfoDict);
                if (currentCharacterData.Relics.Count > 0)
                {
                    relicDropdown1.SetValue(currentCharacterData.Relics[0]);
                }
                relicDropdown1.SetInteractable(_isEditable && !_isInBattle);
            }
            
            if (relicDropdown2 != null)
            {
                relicDropdown2.Initialize(_availableRelics, _weaponInfoDict, _relicInfoDict);
                if (currentCharacterData.Relics.Count > 1)
                {
                    relicDropdown2.SetValue(currentCharacterData.Relics[1]);
                }
                relicDropdown2.SetInteractable(_isEditable && !_isInBattle);
            }
            
            if (relicDropdown3 != null)
            {
                relicDropdown3.Initialize(_availableRelics, _weaponInfoDict, _relicInfoDict);
                if (currentCharacterData.Relics.Count > 2)
                {
                    relicDropdown3.SetValue(currentCharacterData.Relics[2]);
                }
                relicDropdown3.SetInteractable(_isEditable && !_isInBattle);
            }
        }
        
        /// <summary>
        /// Save current dropdown values to character data
        /// </summary>
        public void SaveToCharacterData()
        {
            if (currentCharacterData == null || !_isEditable || _isInBattle) return;
            
            // Save weapons
            if (weaponDropdown1 != null)
            {
                string weapon1 = weaponDropdown1.GetValue();
                currentCharacterData.SetWeapon(0, weapon1);
            }
            
            if (weaponDropdown2 != null)
            {
                string weapon2 = weaponDropdown2.GetValue();
                currentCharacterData.SetWeapon(1, weapon2);
            }
            
            // Save relics
            if (relicDropdown1 != null)
            {
                string relic1 = relicDropdown1.GetValue();
                currentCharacterData.SetRelic(0, relic1);
            }
            
            if (relicDropdown2 != null)
            {
                string relic2 = relicDropdown2.GetValue();
                currentCharacterData.SetRelic(1, relic2);
            }
            
            if (relicDropdown3 != null)
            {
                string relic3 = relicDropdown3.GetValue();
                currentCharacterData.SetRelic(2, relic3);
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
        
        /// <summary>
        /// Get current character data
        /// </summary>
        public PvCharacterEquipmentData GetCharacterData()
        {
            return currentCharacterData;
        }
    }
}

