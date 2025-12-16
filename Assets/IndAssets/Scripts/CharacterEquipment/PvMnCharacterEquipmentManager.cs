using System;
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
        
        private readonly List<string> _availableWeaponNames = new List<string>();
        private readonly List<string> _availableRelicNames = new List<string>();

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
            _availableWeaponNames.Clear();
            _availableRelicNames.Clear();

            if (PvSaveManager.Instance)
            {
                var allWeaponInstances = PvSaveManager.Instance.GetAvailableWeaponInstances();
                foreach (var weaponInstance in allWeaponInstances)
                {
                    var weaponData = _equipmentsCollection.GetWeaponData(weaponInstance.WeaponDataId);
                    _availableWeaponNames.Add(weaponData.weaponName);
                }

                var allRelicInstances = PvSaveManager.Instance.GetAvailableRelicInstances();
                foreach (var relicInstance in allRelicInstances)
                {
                    var relicData = _equipmentsCollection.GetRelicData(relicInstance.RelicDataId);
                    _availableRelicNames.Add(relicData.PassiveName);
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
                    _availableWeaponNames,
                    _availableRelicNames,
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
                equipmentPanel.Initialize(
                    characterData,
                    _availableWeaponNames,
                    _availableRelicNames,
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

