using ProjectCI.CoreSystem.Runtime.Deployment;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;
using ProjectCI.Runtime.GUI.Battle;
using ProjectCI.TacticTool.Formula.Concrete;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Battleground
{
    [CreateAssetMenu(fileName = "PvSoBattlegroundMaker", menuName = "ProjectCI/Battleground/PvSoBattlegroundMaker")]
    public class PvSoBattlegroundMaker : ScriptableObject, IService
    {
        private const float DefaultCellValue = 2.25f;
        public float cellWidth = DefaultCellValue;
        public float cellHeight = DefaultCellValue;
        
        [SerializeField]
        private LayerMask layerMask;
        
        [SerializeField]
        private LayerMask pawnDetectLayerMask;

        [SerializeField]
        private LayerMask cellDetectLayerMask;

        [SerializeField]
        private TacticRpgTool.General.CellPalette cellPalette;
        
        [Header("Battle Manager Settings")]
        [SerializeField]
        private TacticBattleManager battleManagerPrefab;

        [SerializeField]
        private int detectMaxResults = 10;

        [SerializeField]
        private GameObject resourceContainerPrefab;

        [SerializeField]
        private FormulaCollection formulaCollection;

        [SerializeField]
        private PvSoDeploymentController deploymentController;

        [SerializeField]
        private UnityEvent onBattleInitialized;

        public UnityEvent<RaycastHit, Vector2Int, LevelGridBase> gridCreatingRule;

        [NonSerialized] private SquarePresetGrid _levelGrid;
        
        public void ScanAndGenerateBattle()
        {
            var collectedObjects = GameObject.FindGameObjectsWithTag("UICamera");
            var cameraController = FindAnyObjectByType<PvMnBattleCamera>();

            var camera = collectedObjects.Length > 0 ? collectedObjects[0].GetComponent<Camera>() : null;
            if (!camera)
            {
                throw new Exception("ERROR: No UI Camera!");
            }

            var centerPosition = deploymentController.LevelData.LevelStartPosition;
            var gridWidth = deploymentController.LevelData.gridWidth;
            var gridHeight = deploymentController.LevelData.gridHeight;

            GridBattleUtils.GenerateLevelGridFromGround<SquarePresetGrid, PvMnLevelCell>(
                centerPosition,
                cellWidth,
                cellHeight,
                new Vector2Int(gridWidth, gridHeight),
                layerMask,
                cellPalette,
                ref _levelGrid,
                gridCreatingRule
            );

            if (_levelGrid)
            {
                float scaleSize = cellWidth / DefaultCellValue;
                if (!Mathf.Approximately(scaleSize, 1f))
                {
                    _levelGrid.GetAllCells().ForEach(cellMesh =>
                    {
                        var scale = cellMesh.transform.localScale;
                        cellMesh.transform.localScale = scale * scaleSize;
                    });
                }
                Debug.Log($"Successfully generated grid with {gridWidth * gridHeight} cells");

                // 创建 Battle Manager
                var battleManager = GridBattleUtils.CreateBattleManager(
                    battleManagerPrefab,
                    _levelGrid
                );

                var sceneUnits = GridBattleUtils.ScanAreaForObjects<PvMnSceneUnit>(
                    new Vector3(centerPosition.x, 0, centerPosition.z),
                    gridWidth > gridHeight ? gridWidth * 3 : gridHeight * 3,
                    false,
                    pawnDetectLayerMask,
                    detectMaxResults
                );

                Debug.Log($"<color=blue>Found {sceneUnits.Count} SceneUnit(s)</color> in area.");

                foreach (var sceneUnit in sceneUnits)
                {
                    BattleTeam team = sceneUnit.IsFriendly
                        ? BattleTeam.Friendly
                        : BattleTeam.Hostile;

                    var unit = GridBattleUtils.ChangeUnitToBattleUnit<PvMnBattleGeneralUnit>(
                        sceneUnit.gameObject,
                        _levelGrid,
                        sceneUnit.UnitData,
                        team,
                        spawnedUnit =>
                        {
                            spawnedUnit.SetupAttackAbility(sceneUnit.WeaponAttackAbility);
                            spawnedUnit.SetupFollowUpAbility(sceneUnit.WeaponFollowUpAbility);
                            spawnedUnit.SetupCounterAbility(sceneUnit.WeaponCounterAbility);
                            spawnedUnit.SetupSupportAbility(sceneUnit.UnitData.TalentedSupportAbility);
                        },
                        1,
                        cellDetectLayerMask
                    );
                    
                    unit.GenerateNewID();
                    sceneUnit.UnitData.InitializeUnitDataToGridUnit(unit);
                    
                    sceneUnit.InitializeAttributes(unit.RuntimeAttributes);

                    int hitPoint = unit.RuntimeAttributes.GetAttributeValue(formulaCollection.HealthAttributeType);
                    unit.RuntimeAttributes.Health.SetValue(hitPoint, hitPoint);

                    unit.AddComponent<PvMnBattleResourceContainer>();
                    unit.InitializeResourceContainer(camera, cameraController, resourceContainerPrefab);
                    
                    // TODO: Consider if ability need to be assigned
                    // var abilities = unit.GetAbilities();
                    // abilityEquipEvent.Raise(unit, abilities[0]);
                }
                
                battleManager.Initialize();
                onBattleInitialized?.Invoke();
            }
        }
        
        public void Dispose()
        {
            // Empty
        }

        public void Initialize()
        {
            // Empty
        }

        public void Cleanup()
        {
            // Empty
        }
    }
}
