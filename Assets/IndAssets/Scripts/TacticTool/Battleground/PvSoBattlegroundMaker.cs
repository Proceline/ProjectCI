using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;
using Unity.VisualScripting;

namespace ProjectCI.CoreSystem.Runtime.Battleground
{
    [CreateAssetMenu(fileName = "PvSoBattlegroundMaker", menuName = "Project CI/Battleground/PvSoBattlegroundMaker")]
    public class PvSoBattlegroundMaker : ScriptableObject, IService
    {
        public float hexWidth = 2f;
        public float hexHeight = 2f;
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
        private AbilityListUIElementBase abilityListUIPrefab;

        [SerializeField]
        private GameObject resourceContainerPrefab;

        public UnityEvent<RaycastHit, Vector2Int, HexagonPresetGrid> gridCreatingRule;

        [SerializeField] 
        private PvSoAbilityEquipEvent abilityEquipEvent;
        
        public void ScanAndGenerateBattle(Vector3 centerPosition, Camera uiCamera)
        {
            // 使用 GridBattleUtils 生成网格
            var levelGrid = GridBattleUtils.GenerateLevelGridFromGround(
                centerPosition,
                hexWidth,
                hexHeight,
                new Vector2Int(gridWidth, gridHeight),
                layerMask,
                cellPalette,
                gridCreatingRule
            );

            if (levelGrid != null)
            {
                Debug.Log($"Successfully generated grid with {gridWidth * gridHeight} cells");

                // 创建 Battle Manager
                var battleManager = GridBattleUtils.CreateBattleManager(
                    battleManagerPrefab,
                    levelGrid
                );

                if (battleManager != null)
                {
                    Debug.Log("Successfully created Battle Manager");
                    var abilityListUI = Instantiate(abilityListUIPrefab);
                    abilityListUI.InitializeUI(uiCamera);
                }

                var sceneUnits = GridBattleUtils.ScanAreaForObjects<PvMnSceneUnit>(
                    new Vector3(centerPosition.x, 0, centerPosition.z),
                    gridWidth > gridHeight ? gridWidth * 2 : gridHeight * 2,
                    true,
                    pawnDetectLayerMask,
                    detectMaxResults
                );

                Debug.Log($"Found {sceneUnits.Count} SceneUnit(s) in area.");

                foreach (var sceneUnit in sceneUnits)
                {
                    BattleTeam team = sceneUnit.IsFriendly
                        ? BattleTeam.Friendly
                        : BattleTeam.Hostile;

                    var unit = GridBattleUtils.ChangeUnitToBattleUnit<PvMnBattleGeneralUnit>(
                        sceneUnit.gameObject, 
                        levelGrid, 
                        sceneUnit.UnitData, 
                        team,
                        sceneUnit.UnitAbilities,
                        1, 
                        pawnDetectLayerMask
                    );
                    
                    unit.GenerateNewID();
                    sceneUnit.UnitData.InitializeUnitDataToGridUnit(unit);
                    
                    sceneUnit.SetExtraAttributes(unit.RuntimeAttributes);
                    unit.AddComponent<PvMnBattleResourceContainer>();
                    unit.InitializeResourceContainer(uiCamera, resourceContainerPrefab);
                    var abilities = unit.GetAbilities();
                    abilityEquipEvent.Raise(unit, abilities[0]);
                }

                var hoverPawnInfos = GameObject.FindObjectsByType<PvUIHoverPawnInfo>(FindObjectsSortMode.None);
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
