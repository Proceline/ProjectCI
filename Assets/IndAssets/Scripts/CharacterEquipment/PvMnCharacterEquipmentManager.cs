using System.Collections.Generic;
using IndAssets.Scripts.Passives.Relics;
using IndAssets.Scripts.Weapons;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.CharacterEquipment.Data;
using ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment
{
    /// <summary>
    /// Manager for character equipment system
    /// Handles data management and UI coordination
    /// </summary>
    public class PvMnCharacterEquipmentManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private PvMnCharacterPortraitPanel portraitPanel;
        [SerializeField] private PvMnCharacterEquipmentPanel equipmentPanel;
        
        [Header("Data")]
        // Runtime data list - not serialized in inspector, managed at runtime
        private List<PvCharacterEquipmentData> characterDataList = new List<PvCharacterEquipmentData>();
        
        [Header("Available Equipment")]
        [SerializeField] private List<PvSoWeaponData> availableWeapons = new();
        [SerializeField] private List<PvSoPassiveRelic> availableRelics = new();
        
        private bool _isInBattle = false;
        
        // Cached dictionaries for quick lookup
        private readonly Dictionary<string, PvSoWeaponData> _weaponsDict = new();
        private readonly Dictionary<string, PvSoPassiveRelic> _relicsDict = new();
        private readonly List<string> _availableWeaponNames = new List<string>();
        private readonly List<string> _availableRelicNames = new List<string>();
        
        private void Awake()
        {
            BuildEquipmentDictionaries();
        }
        
        private void Start()
        {
            InitializeUI();
        }
        
        /// <summary>
        /// Build dictionaries from available equipment lists
        /// </summary>
        private void BuildEquipmentDictionaries()
        {
            _weaponsDict.Clear();
            _relicsDict.Clear();
            _availableWeaponNames.Clear();
            _availableRelicNames.Clear();
            
            foreach (var weaponInfo in availableWeapons)
            {
                if (weaponInfo != null)
                {
                    var weaponName = weaponInfo.weaponName;
                    _weaponsDict[weaponName] = weaponInfo;
                    _availableWeaponNames.Add(weaponName);
                }
            }
            
            foreach (var relicInfo in availableRelics)
            {
                if (relicInfo != null)
                {
                    string relicName = relicInfo.PassiveName;
                    _relicsDict[relicName] = relicInfo;
                    _availableRelicNames.Add(relicName);
                }
            }
        }
        
        /// <summary>
        /// Initialize UI components
        /// </summary>
        private void InitializeUI()
        {
            if (portraitPanel != null)
            {
                portraitPanel.Initialize(characterDataList, OnCharacterSelected);
            }
            
            if (equipmentPanel != null)
            {
                equipmentPanel.Initialize(
                    null,
                    _availableWeaponNames,
                    _availableRelicNames,
                    _weaponsDict,
                    _relicsDict,
                    OnEquipmentPanelClosed,
                    OnEquipmentDataChanged);
            }
        }
        
        /// <summary>
        /// Set battle state
        /// </summary>
        public void SetBattleState(bool isInBattle)
        {
            _isInBattle = isInBattle;
            
            if (equipmentPanel != null)
            {
                equipmentPanel.SetBattleState(isInBattle);
            }
        }
        
        /// <summary>
        /// Add a character to the system
        /// </summary>
        public void AddCharacter(PvCharacterEquipmentData characterData)
        {
            if (characterData == null) return;
            
            if (!characterDataList.Contains(characterData))
            {
                characterDataList.Add(characterData);
            }
            
            if (portraitPanel != null)
            {
                portraitPanel.AddCharacter(characterData);
            }
        }
        
        /// <summary>
        /// Remove a character from the system
        /// </summary>
        public void RemoveCharacter(PvCharacterEquipmentData characterData)
        {
            if (characterData == null) return;
            
            characterDataList.Remove(characterData);
            
            if (portraitPanel != null)
            {
                portraitPanel.RemoveCharacter(characterData);
            }
        }
        
        /// <summary>
        /// Refresh all UI displays
        /// </summary>
        public void RefreshAllUI()
        {
            if (portraitPanel != null)
            {
                portraitPanel.RefreshPortraits();
            }
            
            if (equipmentPanel != null)
            {
                equipmentPanel.RefreshDisplay();
            }
        }
        
        /// <summary>
        /// Refresh equipment panel only
        /// </summary>
        public void RefreshEquipmentPanel()
        {
            if (equipmentPanel != null)
            {
                equipmentPanel.RefreshDisplay();
            }
        }
        
        /// <summary>
        /// Refresh portrait panel only
        /// </summary>
        public void RefreshPortraitPanel()
        {
            if (portraitPanel != null)
            {
                portraitPanel.RefreshPortraits();
            }
        }
        
        /// <summary>
        /// Update available equipment lists and rebuild dictionaries
        /// </summary>
        public void UpdateAvailableEquipment(List<PvSoWeaponData> weaponInfos, List<PvSoPassiveRelic> relicInfos)
        {
            availableWeapons = weaponInfos ?? new List<PvSoWeaponData>();
            availableRelics = relicInfos ?? new List<PvSoPassiveRelic>();
            
            BuildEquipmentDictionaries();
            
            // Refresh UI with new equipment lists
            RefreshAllUI();
        }
        
        private void OnCharacterSelected(PvCharacterEquipmentData characterData)
        {
            if (equipmentPanel != null && characterData != null)
            {
                equipmentPanel.Initialize(
                    characterData,
                    _availableWeaponNames,
                    _availableRelicNames,
                    _weaponsDict,
                    _relicsDict,
                    OnEquipmentPanelClosed,
                    OnEquipmentDataChanged);
                equipmentPanel.ShowPanel();
            }
        }
        
        private void OnEquipmentPanelClosed()
        {
            // Handle panel closed if needed
        }
        
        private void OnEquipmentDataChanged()
        {
            // Save data when changed
            if (equipmentPanel != null)
            {
                equipmentPanel.SaveToCharacterData();
            }
            
            // Refresh portrait panel to reflect changes
            if (portraitPanel != null)
            {
                portraitPanel.UpdateAllPortraits();
            }
        }
        
        /// <summary>
        /// Get all character data
        /// </summary>
        public List<PvCharacterEquipmentData> GetAllCharacterData()
        {
            return new List<PvCharacterEquipmentData>(characterDataList);
        }
        
        /// <summary>
        /// Get character data by name
        /// </summary>
        public PvCharacterEquipmentData GetCharacterDataByName(string characterName)
        {
            return characterDataList.Find(data => data.CharacterName == characterName);
        }
        
        /// <summary>
        /// Create a new character data instance
        /// </summary>
        public PvCharacterEquipmentData CreateCharacterData(string characterName)
        {
            var newData = new PvCharacterEquipmentData(characterName);
            AddCharacter(newData);
            return newData;
        }
        
        /// <summary>
        /// Load character data from save file (placeholder for future implementation)
        /// </summary>
        public void LoadCharacterData()
        {
            // TODO: Implement load from JSON/Binary file
            // Example structure:
            // string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, "CharacterEquipment.json"));
            // characterDataList = JsonUtility.FromJson<List<PvCharacterEquipmentData>>(json);
        }
        
        /// <summary>
        /// Save character data to file (placeholder for future implementation)
        /// </summary>
        public void SaveCharacterData()
        {
            // TODO: Implement save to JSON/Binary file
            // Example structure:
            // string json = JsonUtility.ToJson(characterDataList, true);
            // File.WriteAllText(Path.Combine(Application.persistentDataPath, "CharacterEquipment.json"), json);
        }
    }
}

