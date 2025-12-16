using System.Collections.Generic;
using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.DependencyInjection;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI;
using ProjectCI.CoreSystem.Runtime.Saving;
using ProjectCI.CoreSystem.Runtime.Saving.Data;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment
{
    /// <summary>
    /// Manager for character equipment system
    /// Handles data management and UI coordination
    /// </summary>
    [StaticInjectableTarget]
    public class PvMnCharacterEquipmentManager : MonoBehaviour
    {
        [SerializeField] private bool setupWhileSpawned = false;
        
        [Header("UI References")]
        [SerializeField] private PvMnCharacterPortraitPanel portraitPanel;
        [SerializeField] private PvMnCharacterEquipmentPanel equipmentPanel;
        
        [Inject] private static PvSoWeaponAndRelicCollection _equipmentsCollection;
        
        private bool _isInBattle = false;
        
        // Store instanceIds and display names separately
        private readonly List<string> _availableWeaponInstanceIds = new List<string>();
        private readonly List<string> _availableWeaponDisplayNames = new List<string>();
        private readonly List<string> _availableRelicInstanceIds = new List<string>();
        private readonly List<string> _availableRelicDisplayNames = new List<string>();

        private void Start()
        {
            if (setupWhileSpawned)
            {
                InitializeAndUpdateEquipmentUI();
            }
        }

        public void InitializeAndUpdateEquipmentUI()
        {
            BuildEquipmentDictionaries();
            InitializeUI();
        }
        
        /// <summary>
        /// Build dictionaries from available equipment lists
        /// </summary>
        private void BuildEquipmentDictionaries()
        {
            _availableWeaponInstanceIds.Clear();
            _availableWeaponDisplayNames.Clear();
            _availableRelicInstanceIds.Clear();
            _availableRelicDisplayNames.Clear();

            if (PvSaveManager.Instance)
            {
                var allWeaponInstances = PvSaveManager.Instance.GetAvailableWeaponInstances();
                foreach (var weaponInstance in allWeaponInstances)
                {
                    var weaponData = _equipmentsCollection.GetWeaponData(weaponInstance.WeaponDataId);
                    if (weaponData != null)
                    {
                        _availableWeaponInstanceIds.Add(weaponInstance.InstanceId);
                        _availableWeaponDisplayNames.Add(weaponData.weaponName);
                    }
                }

                var allRelicInstances = PvSaveManager.Instance.GetAvailableRelicInstances();
                foreach (var relicInstance in allRelicInstances)
                {
                    var relicData = _equipmentsCollection.GetRelicData(relicInstance.RelicDataId);
                    if (relicData != null)
                    {
                        _availableRelicInstanceIds.Add(relicInstance.InstanceId);
                        _availableRelicDisplayNames.Add(relicData.PassiveName);
                    }
                }
            }
        }
        
        /// <summary>
        /// Initialize UI components
        /// </summary>
        private void InitializeUI()
        {
            if (!PvSaveManager.Instance) return;
            
            if (portraitPanel != null)
            {
                portraitPanel.Initialize(OnCharacterSelected);
            }
            
            if (equipmentPanel != null)
            {
                equipmentPanel.Initialize(
                    null,
                    _availableWeaponInstanceIds,
                    _availableWeaponDisplayNames,
                    _availableRelicInstanceIds,
                    _availableRelicDisplayNames,
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
        /// Refresh all UI displays
        /// </summary>
        public void RefreshAllUI()
        {
            RefreshPortraitPanel();
            
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
        public void UpdateAvailableEquipment()
        {
            BuildEquipmentDictionaries();
            
            // Refresh UI with new equipment lists
            RefreshAllUI();
        }
        
        private void OnCharacterSelected(PvCharacterSaveData characterData)
        {
            if (equipmentPanel != null && characterData != null)
            {
                // Build lists including currently equipped items for this character
                var weaponInstanceIds = new List<string>(_availableWeaponInstanceIds);
                var weaponDisplayNames = new List<string>(_availableWeaponDisplayNames);
                var relicInstanceIds = new List<string>(_availableRelicInstanceIds);
                var relicDisplayNames = new List<string>(_availableRelicDisplayNames);
                
                // Add currently equipped weapons if not already in the list
                if (PvSaveManager.Instance != null && PvSaveManager.Instance.CurrentSaveData != null)
                {
                    foreach (var weaponInstanceId in characterData.WeaponInstanceIds)
                    {
                        if (!string.IsNullOrEmpty(weaponInstanceId) && !weaponInstanceIds.Contains(weaponInstanceId))
                        {
                            var weaponInstance = PvSaveManager.Instance.CurrentSaveData.GetWeaponInstance(weaponInstanceId);
                            if (weaponInstance != null)
                            {
                                var weaponData = _equipmentsCollection.GetWeaponData(weaponInstance.WeaponDataId);
                                if (weaponData != null)
                                {
                                    weaponInstanceIds.Add(weaponInstanceId);
                                    weaponDisplayNames.Add(weaponData.weaponName);
                                }
                            }
                        }
                    }
                    
                    // Add currently equipped relics if not already in the list
                    foreach (var relicInstanceId in characterData.RelicInstanceIds)
                    {
                        if (!string.IsNullOrEmpty(relicInstanceId) && !relicInstanceIds.Contains(relicInstanceId))
                        {
                            var relicInstance = PvSaveManager.Instance.CurrentSaveData.GetRelicInstance(relicInstanceId);
                            if (relicInstance != null)
                            {
                                var relicData = _equipmentsCollection.GetRelicData(relicInstance.RelicDataId);
                                if (relicData != null)
                                {
                                    relicInstanceIds.Add(relicInstanceId);
                                    relicDisplayNames.Add(relicData.PassiveName);
                                }
                            }
                        }
                    }
                }
                
                equipmentPanel.Initialize(
                    characterData,
                    weaponInstanceIds,
                    weaponDisplayNames,
                    relicInstanceIds,
                    relicDisplayNames,
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

