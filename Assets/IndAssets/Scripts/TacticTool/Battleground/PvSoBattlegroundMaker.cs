using System;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;
using ProjectCI.Runtime.GUI.Battle;
using UnityEngine;
using UnityEngine.Events;
using Unity.VisualScripting;

namespace ProjectCI.CoreSystem.Runtime.Battleground
{
    [CreateAssetMenu(fileName = "PvSoBattlegroundMaker", menuName = "Project CI/Battleground/PvSoBattlegroundMaker")]
    public class PvSoBattlegroundMaker : ScriptableObject, IService
    {
        private const float DefaultCellValue = 2.25f;
        public float cellWidth = DefaultCellValue;
        public float cellHeight = DefaultCellValue;
        public int gridWidth = 5;
        public int gridHeight = 5;
        
        [SerializeField]
        private LayerMask layerMask;
        
        [SerializeField]
        private LayerMask pawnDetectLayerMask;
        
        [SerializeField]
        private TacticRpgTool.General.CellPalette cellPalette;
        
        [Header("Battle Manager Settings")]
        [SerializeField]
        private TacticBattleManager battleManagerPrefab;

        [SerializeField]
        private int detectMaxResults = 10;

        [SerializeField]
        private GameObject resourceContainerPrefab;

        public UnityEvent<RaycastHit, Vector2Int, LevelGridBase> gridCreatingRule;

        [NonSerialized] private SquarePresetGrid _levelGrid;
        
        public void ScanAndGenerateBattle(Vector3 centerPosition, Camera uiCamera)
        {
            GridBattleUtils.GenerateLevelGridFromGround(
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
                    gridWidth > gridHeight ? gridWidth * 2 : gridHeight * 2,
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
                        spawnedUnit => spawnedUnit.SetupAbilities(sceneUnit.UnitAbilities),
                        1,
                        pawnDetectLayerMask
                    );
                    
                    unit.GenerateNewID();
                    sceneUnit.UnitData.InitializeUnitDataToGridUnit(unit);
                    
                    sceneUnit.SetExtraAttributes(unit.RuntimeAttributes);
                    unit.AddComponent<PvMnBattleResourceContainer>();
                    unit.InitializeResourceContainer(uiCamera, resourceContainerPrefab);
                    
                    // TODO: Consider if ability need to be assigned
                    // var abilities = unit.GetAbilities();
                    // abilityEquipEvent.Raise(unit, abilities[0]);
                }

                var hoverPawnInfos = FindObjectsByType<PvUIHoverPawnInfo>(FindObjectsSortMode.None);
                foreach (var hoverPawnInfo in hoverPawnInfos)
                {
                    hoverPawnInfo.Initialize();
                }
                
                battleManager.Initialize();
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
