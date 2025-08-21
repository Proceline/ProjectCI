using System;
using ProjectCI.CoreSystem.Runtime.Battleground;
using ProjectCI.CoreSystem.Runtime.Services;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    public class HexagonGridScannerWindow : EditorWindow
    {
        private Vector3 _centerPosition = new Vector3(-3, 10, 3);

        private LayerMask _pawnDetectLayerMask = 0;

        [SerializeField]
        private GameObject resourceContainerPrefab;

        private readonly ServiceLocator<PvSoBattlegroundMaker> _battlegroundMaker = new();

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
            _centerPosition = EditorGUILayout.Vector3Field("Center Position", _centerPosition);

            EditorGUILayout.Space();
            
            string[] pawnLayerNames = InternalEditorUtility.layers;
            int testMaskValue = _pawnDetectLayerMask.value;
            testMaskValue = EditorGUILayout.MaskField("Layer Mask", testMaskValue, pawnLayerNames);
            _pawnDetectLayerMask.value = testMaskValue;

            EditorGUILayout.Space();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("UI Prefabs", EditorStyles.boldLabel);
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
            var collectedObjects = GameObject.FindGameObjectsWithTag("UICamera");
            if (collectedObjects.Length > 0)
            {
                Camera camera = collectedObjects[0].GetComponent<Camera>();
                if (camera == null)
                {
                    throw new NullReferenceException("ERROR: UI Camera not defined in Scene!");
                }
                _battlegroundMaker.Service.ScanAndGenerateBattle(_centerPosition, camera);
            }
        }
    }
} 