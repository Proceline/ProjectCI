using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.Saving;
using IndAssets.Scripts.Weapons;
using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.DependencyInjection;

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
        
        [Header("Deployed Units")]
        [SerializeField] private List<PvSoBattleUnitData> deployedUnits = new List<PvSoBattleUnitData>();
        
        [Header("Slot Mapping")]
        [Tooltip("Maps unit index to slot index. If empty, units will be assigned to available slots in order.")]
        [SerializeField] private List<int> unitToSlotMapping = new List<int>();
        
        [System.NonSerialized]
        private Dictionary<int, PvMnSceneUnit> _slotToUnitMap = new Dictionary<int, PvMnSceneUnit>();
        
        [System.NonSerialized]
        private Dictionary<PvMnSceneUnit, int> _unitToSlotMap = new Dictionary<PvMnSceneUnit, int>();
        
        [Inject]
        private static PvSoWeaponAndRelicCollection WeaponAndRelicCollection;

        /// <summary>
        /// Get level data
        /// </summary>
        public PvSoLevelData LevelData => levelData;
        
        /// <summary>
        /// Get deployed units list
        /// </summary>
        public List<PvSoBattleUnitData> DeployedUnits => deployedUnits;
        
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

            var bodyMeshPrefab = WeaponAndRelicCollection.GetDefaultBodyMeshPrefab();
            if (bodyMeshPrefab)
            {
                var bodyMesh = Instantiate(bodyMeshPrefab, sceneUnit.transform);
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
            _slotToUnitMap[slot.SlotIndex] = sceneUnit;
            _unitToSlotMap[sceneUnit] = slot.SlotIndex;
            deployedUnits.Add(unitData);
            unitToSlotMapping.Add(slot.SlotIndex);
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
                slot.OccupiedUnit = newUnit;
                _slotToUnitMap[slotIndex] = newUnit;
                _unitToSlotMap[newUnit] = slotIndex;
                
                // Update deployed units list if this slot was already assigned
                int unitIndex = deployedUnits.FindIndex(u => u == oldUnit?.UnitData as PvSoBattleUnitData);
                if (unitIndex >= 0 && unitIndex < deployedUnits.Count)
                {
                    deployedUnits[unitIndex] = newUnitData;
                }
            }
            
            return newUnit;
        }
        
        /// <summary>
        /// Replace unit by unit reference
        /// </summary>
        /// <param name="oldUnit">Old scene unit to replace</param>
        /// <param name="newUnitData">New unit data to deploy</param>
        /// <param name="parent">Parent transform for spawned units</param>
        /// <param name="levelGrid">Optional level grid for grid-based positioning</param>
        /// <returns>New scene unit, or null if failed</returns>
        public PvMnSceneUnit ReplaceUnit(PvMnSceneUnit oldUnit, PvSoBattleUnitData newUnitData, Transform parent = null, LevelGridBase levelGrid = null)
        {
            if (oldUnit == null || newUnitData == null)
            {
                Debug.LogError("Old unit or new unit data is null!");
                return null;
            }
            
            if (!_unitToSlotMap.TryGetValue(oldUnit, out int slotIndex))
            {
                Debug.LogError("Old unit is not tracked by deployment controller!");
                return null;
            }
            
            return ReplaceUnitAtSlot(slotIndex, newUnitData, parent, levelGrid);
        }
        
        /// <summary>
        /// Remove unit from slot
        /// </summary>
        /// <param name="slotIndex">Slot index to remove unit from</param>
        public void RemoveUnitFromSlot(int slotIndex)
        {
            if (_slotToUnitMap.TryGetValue(slotIndex, out var unit))
            {
                var slot = levelData?.GetSlotByIndex(slotIndex, true);
                if (slot != null)
                {
                    slot.ClearOccupiedUnit();
                }
                
                _unitToSlotMap.Remove(unit);
                _slotToUnitMap.Remove(slotIndex);
                
                if (unit != null)
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
            var unitsToRemove = new List<PvMnSceneUnit>(_unitToSlotMap.Keys);
            foreach (var unit in unitsToRemove)
            {
                if (unit != null)
                {
                    Destroy(unit.gameObject);
                }
            }
            
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
        /// Get slot index for unit at given index
        /// </summary>
        private int GetSlotIndexForUnit(int unitIndex)
        {
            if (unitIndex < unitToSlotMapping.Count && unitToSlotMapping[unitIndex] >= 0)
            {
                return unitToSlotMapping[unitIndex];
            }
            
            // Auto-assign to first available slot
            return unitIndex;
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
            var weaponsList = new List<PvSoWeaponData>();
            
            foreach (var weaponInstanceId in characterData.WeaponInstanceIds)
            {
                if (string.IsNullOrEmpty(weaponInstanceId)) continue;
                
                var weaponInstance = saveManager.CurrentSaveData.GetWeaponInstance(weaponInstanceId);
                if (weaponInstance != null)
                {
                    var weaponData = WeaponAndRelicCollection.GetWeaponData(weaponInstance.WeaponDataId);
                    if (weaponData != null)
                    {
                        weaponsList.Add(weaponData);
                    }
                }
            }
            
            sceneUnit.Initialize(weaponsList);
        }
        
        /// <summary>
        /// Get unit at slot index
        /// </summary>
        public PvMnSceneUnit GetUnitAtSlot(int slotIndex)
        {
            _slotToUnitMap.TryGetValue(slotIndex, out var unit);
            return unit;
        }
        
        /// <summary>
        /// Get slot index for unit
        /// </summary>
        public int GetSlotIndexForUnit(PvMnSceneUnit unit)
        {
            _unitToSlotMap.TryGetValue(unit, out int slotIndex);
            return slotIndex;
        }
    }
}
