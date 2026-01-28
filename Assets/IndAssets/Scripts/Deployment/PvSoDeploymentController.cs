using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.Saving;
using IndAssets.Scripts.Weapons;
using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.DependencyInjection;
using System;

namespace ProjectCI.CoreSystem.Runtime.Deployment
{
    /// <summary>
    /// Deployment controller that manages which units are deployed and their positions
    /// </summary>
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "NewDeploymentController", menuName = "ProjectCI/Deployment/Deployment Controller", order = 1)]
    public class PvSoDeploymentController : ScriptableObject
    {
        [Header("Deployment Configuration")]
        [SerializeField] private PvSoLevelData levelData;
        [SerializeField] private PvMnSceneUnit sceneUnitPrefab;
        [SerializeField] private GameObject startPivotMarkerPrefab;

        [NonSerialized] private Dictionary<PvSoBattleUnitData, PvMnSceneUnit> _deployedUnits = new();
        
        [NonSerialized]
        private Dictionary<int, PvSoBattleUnitData> _slotToUnitMap = new Dictionary<int, PvSoBattleUnitData>();
        
        [NonSerialized]
        private Dictionary<PvSoBattleUnitData, int> _unitToSlotMap = new Dictionary<PvSoBattleUnitData, int>();
        
        [Inject]
        private static PvSoWeaponAndRelicCollection WeaponAndRelicCollection;

        [NonSerialized]
        private readonly List<GameObject> _loadedMarkers = new();

        /// <summary>
        /// Get level data
        /// </summary>
        public PvSoLevelData LevelData => levelData;
        
        public void ShowAllSpawnHints()
        {
            foreach (var slotData in levelData.FriendlySlots)
            {
                if (slotData.UseWorldPosition)
                {
                    var marker = Instantiate(startPivotMarkerPrefab, slotData.WorldPosition, Quaternion.identity);
                    _loadedMarkers.Add(marker);
                }
            }
        }

        public void RemoveAllSpawnHints()
        {
            foreach (var marker in _loadedMarkers)
            {
                Destroy(marker);
            }
        }

        public void PutCameraOnPositionOnStarted()
        {
            levelData.PutCameraOnPosition();
        }

        /// <summary>
        /// Deploy unit to available friendly slot
        /// </summary>
        /// <param name="unitData">Unit data to deploy</param>
        public void DeployUnit(PvSoBattleUnitData unitData)
        {
            if (!levelData || !unitData)
            {
                Debug.LogError("Level data or unit data is null!");
                return;
            }

            if (_deployedUnits.ContainsKey(unitData))
            {
                Debug.LogError("This Unit already deployed!");
                return;
            }

            var slot = levelData.GetAvailableFriendlySlot();
            if (slot == null)
            {
                Debug.LogError("No available slot found!");
                return;
            }
            
            var sceneUnit = SpawnUnitAtSlot(unitData, slot, null, null);
            if (sceneUnit == null)
            {
                Debug.LogError("Failed to spawn unit!");
                return;
            }

            var bodyMeshPrefab = unitData.BodyMeshPrefab;
            if (bodyMeshPrefab)
            {
                var bodyMesh = Instantiate(bodyMeshPrefab, sceneUnit.transform).GetComponent<PvMnMeshPartController>();
                if (!bodyMesh)
                {
                    throw new System.Exception($"ERROR: This Mesh doesn't have {nameof(PvMnMeshPartController)}");
                }

                bodyMesh.transform.localPosition = Vector3.zero;
                bodyMesh.transform.localRotation = Quaternion.identity;
                bodyMesh.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);

                var headPrefab = unitData.HeadMeshPrefab;
                bodyMesh.InstantiatePartPrefab("head", headPrefab);
            }

            // Load equipment from save system if available
            if (PvSaveManager.Instance && PvSaveManager.Instance.IsInitialized)
            {
                LoadEquipmentFromSave(sceneUnit, unitData);
            }

            slot.OccupiedUnit = sceneUnit;
            _slotToUnitMap[slot.SlotIndex] = unitData;
            _unitToSlotMap[unitData] = slot.SlotIndex;
            _deployedUnits.Add(unitData, sceneUnit);
        }
        
        /// <summary>
        /// Replace unit at specific slot index
        /// </summary>
        /// <param name="slotIndex">Slot index to replace unit at</param>
        /// <param name="newUnitData">New unit data to deploy</param>
        /// <param name="parent">Parent transform for spawned units</param>
        /// <param name="levelGrid">Optional level grid for grid-based positioning</param>
        /// <returns>New scene unit, or null if failed</returns>
        public PvMnSceneUnit ReplaceUnitAtSlot(int slotIndex, PvSoBattleUnitData newUnitData, Transform parent = null, LevelGridBase levelGrid = null)
        {
            if (levelData == null || newUnitData == null)
            {
                Debug.LogError("Level data or unit data is null!");
                return null;
            }
            
            var slot = levelData.GetSlotByIndex(slotIndex, true);
            if (slot == null)
            {
                Debug.LogError($"Slot {slotIndex} not found!");
                return null;
            }
            
            // Remove old unit if exists
            if (_slotToUnitMap.TryGetValue(slotIndex, out var oldUnit))
            {
                RemoveUnitFromSlot(slotIndex);
            }
            
            // Spawn new unit
            var newUnit = SpawnUnitAtSlot(newUnitData, slot, parent, levelGrid);
            if (newUnit != null)
            {
                var oldSpawnedUnit = slot.OccupiedUnit;
                var oldUnitData = _slotToUnitMap[slotIndex];
                _unitToSlotMap.Remove(oldUnitData);
                _deployedUnits.Remove(oldUnitData);
                Destroy(oldSpawnedUnit.gameObject);

                slot.OccupiedUnit = newUnit;
                _slotToUnitMap[slotIndex] = newUnitData;
                _unitToSlotMap[newUnitData] = slotIndex;
                _deployedUnits[newUnitData] = newUnit;
            }
            
            return newUnit;
        }
        
        /// <summary>
        /// Remove unit from slot
        /// </summary>
        /// <param name="slotIndex">Slot index to remove unit from</param>
        public void RemoveUnitFromSlot(int slotIndex)
        {
            if (_slotToUnitMap.TryGetValue(slotIndex, out var unitData))
            {
                var slot = levelData?.GetSlotByIndex(slotIndex, true);
                if (slot != null)
                {
                    slot.ClearOccupiedUnit();
                }
                
                _unitToSlotMap.Remove(unitData);
                _slotToUnitMap.Remove(slotIndex);
                
                if (unitData && _deployedUnits.TryGetValue(unitData, out var unit))
                {
                    Destroy(unit.gameObject);
                }
            }
        }
        
        /// <summary>
        /// Clear all deployed units
        /// </summary>
        public void ClearAllUnits()
        {
            var unitsToRemove = new List<PvMnSceneUnit>(_deployedUnits.Values);
            foreach (var unit in unitsToRemove)
            {
                if (unit != null)
                {
                    Destroy(unit.gameObject);
                }
            }

            _deployedUnits.Clear();
            _slotToUnitMap.Clear();
            _unitToSlotMap.Clear();
            
            if (levelData != null)
            {
                foreach (var slot in levelData.FriendlySlots)
                {
                    slot.ClearOccupiedUnit();
                }
            }
        }
        
        /// <summary>
        /// Spawn unit at specified slot
        /// </summary>
        private PvMnSceneUnit SpawnUnitAtSlot(PvSoBattleUnitData unitData, PvDeploymentSlot slot, Transform parent, LevelGridBase levelGrid)
        {
            // Calculate world position
            Vector3 spawnPosition = CalculateSpawnPosition(slot, levelGrid);
            
            // Instantiate scene unit
            var sceneUnit = Instantiate(sceneUnitPrefab, spawnPosition, Quaternion.identity, parent);
            
            // Set unit data
            sceneUnit.UnitData = unitData;
            sceneUnit.IsFriendly = true;
            
            // Initialize unit
            sceneUnit.GenerateNewID();
            
            return sceneUnit;
        }
        
        /// <summary>
        /// Calculate spawn position for slot
        /// </summary>
        private Vector3 CalculateSpawnPosition(PvDeploymentSlot slot, LevelGridBase levelGrid)
        {
            if (slot.UseWorldPosition)
            {
                return slot.WorldPosition;
            }
            
            // Try to get position from grid
            if (levelGrid != null)
            {
                var cell = levelGrid[slot.GridPosition];
                if (cell != null)
                {
                    return cell.transform.position;
                }
            }
            
            // Fallback to world position or grid position as world position
            return slot.WorldPosition != Vector3.zero ? slot.WorldPosition : new Vector3(slot.GridPosition.x, 0, slot.GridPosition.y);
        }
        
        /// <summary>
        /// Load equipment (weapons, abilities) from save system
        /// </summary>
        private void LoadEquipmentFromSave(PvMnSceneUnit sceneUnit, PvSoBattleUnitData unitData)
        {
            var saveManager = PvSaveManager.Instance;
            var characterData = saveManager.GetCharacterEquipmentData(unitData.EntryId);
            
            if (characterData == null)
            {
                Debug.LogWarning($"No save data found for character: {unitData.EntryId}");
                return;
            }

            // Load weapons
            var weaponInstanceId = characterData.WeaponInstanceId;
            if (string.IsNullOrEmpty(weaponInstanceId))
            {
                return;
            }

            var weaponInstance = saveManager.CurrentSaveData.GetWeaponInstance(weaponInstanceId);
            if (weaponInstance != null)
            {
                var weaponData = WeaponAndRelicCollection.GetWeaponData(weaponInstance.WeaponDataId);
                if (weaponData)
                {
                    sceneUnit.Initialize(weaponData);
                }
            }
        }
    }
}
