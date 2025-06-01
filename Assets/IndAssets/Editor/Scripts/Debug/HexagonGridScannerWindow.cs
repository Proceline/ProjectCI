using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using UnityEditorInternal;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI;
using Unity.VisualScripting;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    public class HexagonGridScannerWindow : EditorWindow
    {
        private Vector3 centerPosition = new Vector3(-3, 10, 3);
        private float hexWidth = 2f;
        private float hexHeight = 2f;
        private int gridWidth = 5;
        private int gridHeight = 5;
        private float maxDistance = 100f;
        private LayerMask layerMask = 0;
        [SerializeField]
        private Runtime.TacticRpgTool.General.CellPalette cellPalette;

        [Header("Battle Manager Settings")]
        [SerializeField]
        private TacticBattleManager battleManagerPrefab;

        private LayerMask pawnDetectLayerMask = 0;
        private int detectMaxResults = 10;

        [SerializeField]
        private AbilityListUIElementBase abilityListUIPrefab;

        [SerializeField]
        private GameObject resourceContainerPrefab;

        [SerializeField]
        private Camera uiCamera;

        [MenuItem("ProjectCI Tools/Debug/Hexagon Grid Scanner")]
        public static void ShowWindow()
        {
            GetWindow<HexagonGridScannerWindow>("Hexagon Grid Scanner");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Hexagon Grid Scanner", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 扫描参数
            EditorGUILayout.LabelField("Scan Parameters", EditorStyles.boldLabel);
            centerPosition = EditorGUILayout.Vector3Field("Center Position", centerPosition);
            hexWidth = EditorGUILayout.FloatField("Hexagon Width", hexWidth);
            hexHeight = EditorGUILayout.FloatField("Hexagon Height", hexHeight);
            gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
            gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
            maxDistance = EditorGUILayout.FloatField("Max Distance", maxDistance);
            string[] layerNames = InternalEditorUtility.layers;
            int maskValue = layerMask.value;
            maskValue = EditorGUILayout.MaskField("Layer Mask", maskValue, layerNames);
            layerMask.value = maskValue;

            EditorGUILayout.Space();

            // 网格生成参数
            EditorGUILayout.LabelField("Grid Generation Parameters", EditorStyles.boldLabel);
            cellPalette = EditorGUILayout.ObjectField("Cell Palette", 
                cellPalette, typeof(Runtime.TacticRpgTool.General.CellPalette), true) 
                as Runtime.TacticRpgTool.General.CellPalette;

            EditorGUILayout.Space();

            // Battle Manager 参数
            EditorGUILayout.LabelField("Battle Manager Parameters", EditorStyles.boldLabel);
            battleManagerPrefab = EditorGUILayout.ObjectField("Battle Manager Prefab", battleManagerPrefab, typeof(TacticBattleManager), true) as TacticBattleManager;
            
            string[] pawnLayerNames = InternalEditorUtility.layers;
            int testMaskValue = pawnDetectLayerMask.value;
            testMaskValue = EditorGUILayout.MaskField("Layer Mask", testMaskValue, pawnLayerNames);
            pawnDetectLayerMask.value = testMaskValue;
            detectMaxResults = EditorGUILayout.IntField("Max Results", detectMaxResults);

            EditorGUILayout.Space();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("UI Prefabs", EditorStyles.boldLabel);
            abilityListUIPrefab = (AbilityListUIElementBase)EditorGUILayout.ObjectField("Ability List UI Prefab", abilityListUIPrefab, typeof(AbilityListUIElementBase), false);
            uiCamera = (Camera)EditorGUILayout.ObjectField("UI Camera", uiCamera, typeof(Camera), true);
            resourceContainerPrefab = (GameObject)EditorGUILayout.ObjectField("Resource Container Prefab", resourceContainerPrefab, typeof(GameObject), false);

            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Generate Battle"))
            {
                ScanAndGenerateBattle();
            }
            GUI.enabled = true;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Scan button only works in Play Mode", MessageType.Warning);
            }
        }

        private void ScanAndGenerateBattle()
        {
            if (cellPalette == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Cell Palette", "OK");
                return;
            }

            if (battleManagerPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Battle Manager Prefab", "OK");
                return;
            }

            // 使用 GridBattleUtils 生成网格
            var levelGrid = GridBattleUtils.GenerateLevelGridFromGround<HexagonPresetGrid>(
                centerPosition,
                hexWidth,
                hexHeight,
                new Vector2Int(gridWidth, gridHeight),
                layerMask,
                cellPalette
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

                    unit.SetUnitData(sceneUnit.UnitData);
                    unit.AddComponent<PvMnBattleResourceContainer>();
                    unit.InitializeResourceContainer(uiCamera, resourceContainerPrefab);
                }
                
                battleManager.Initialize();
            }
        }
    }
} 