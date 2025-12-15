using UnityEditor;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.SceneTransition;
using System.Collections.Generic;

namespace ProjectCI.CoreSystem.Editor.SceneTransition
{
    /// <summary>
    /// Custom editor for PvSoSceneManagerConfig
    /// Provides management interface for scene configurations and prefab assignment
    /// </summary>
    [CustomEditor(typeof(PvSoSceneManagerConfig))]
    public class PvSoSceneManagerConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _loadingSceneAssetProperty;
        private SerializedProperty _sceneConfigsProperty;
        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            _loadingSceneAssetProperty = serializedObject.FindProperty("loadingSceneAsset");
            _sceneConfigsProperty = serializedObject.FindProperty("sceneConfigs");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene Transition Manager Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Loading Scene Asset
            EditorGUILayout.PropertyField(_loadingSceneAssetProperty, new GUIContent("Loading Scene Asset", "The loading scene that will be shown during scene transitions"));

            // Display loading scene name (read-only)
            PvSoSceneManagerConfig config = (PvSoSceneManagerConfig)target;
            if (config.LoadingSceneAsset != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Loading Scene Name:", GUILayout.Width(150));
                EditorGUILayout.LabelField(config.LoadingSceneName, EditorStyles.helpBox);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Please assign a Loading Scene Asset", MessageType.Warning);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);

            // Scene Configurations Section
            EditorGUILayout.LabelField($"Scene Configurations ({_sceneConfigsProperty.arraySize})", EditorStyles.boldLabel);

            // Buttons for managing scene configs
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add New Scene Config", GUILayout.Height(25)))
            {
                AddNewSceneConfig();
            }
            if (GUILayout.Button("Scan All Scene Configs", GUILayout.Height(25)))
            {
                ScanAllSceneConfigs();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Display scene configs list
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < _sceneConfigsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                var sceneConfigProperty = _sceneConfigsProperty.GetArrayElementAtIndex(i);
                var sceneConfig = sceneConfigProperty.objectReferenceValue as PvSoSceneConfig;

                EditorGUILayout.BeginHorizontal();
                
                // Scene Config field
                EditorGUILayout.PropertyField(sceneConfigProperty, new GUIContent($"Scene Config {i + 1}"), true);

                // Remove button
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    _sceneConfigsProperty.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                // Display scene info if config is assigned
                if (sceneConfig != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUI.indentLevel++;

                    EditorGUILayout.LabelField($"Scene: {sceneConfig.SceneName}", EditorStyles.miniLabel);
                    
                    var prefabsCount = sceneConfig.PrefabsToInstantiate != null ? sceneConfig.PrefabsToInstantiate.Count : 0;
                    EditorGUILayout.LabelField($"Prefabs to Instantiate: {prefabsCount}", EditorStyles.miniLabel);

                    // Button to open scene config for editing
                    if (GUILayout.Button("Edit Scene Config", GUILayout.Height(20)))
                    {
                        Selection.activeObject = sceneConfig;
                        EditorGUIUtility.PingObject(sceneConfig);
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            // Help box
            if (_sceneConfigsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No scene configurations added. Click 'Add New Scene Config' or 'Scan All Scene Configs' to get started.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Add a new empty scene config slot
        /// </summary>
        private void AddNewSceneConfig()
        {
            _sceneConfigsProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Scan project for all PvSoSceneConfig assets and add them to the list
        /// </summary>
        private void ScanAllSceneConfigs()
        {
            List<PvSoSceneConfig> foundConfigs = new List<PvSoSceneConfig>();

            // Find all PvSoSceneConfig assets in Assets folder
            string[] guids = AssetDatabase.FindAssets("t:PvSoSceneConfig", new[] { "Assets" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PvSoSceneConfig config = AssetDatabase.LoadAssetAtPath<PvSoSceneConfig>(path);
                if (config != null)
                {
                    foundConfigs.Add(config);
                }
            }

            // Update the serialized property
            _sceneConfigsProperty.ClearArray();
            for (int i = 0; i < foundConfigs.Count; i++)
            {
                _sceneConfigsProperty.InsertArrayElementAtIndex(i);
                _sceneConfigsProperty.GetArrayElementAtIndex(i).objectReferenceValue = foundConfigs[i];
            }

            serializedObject.ApplyModifiedProperties();

            Debug.Log($"Scanned and found {foundConfigs.Count} scene config assets.");
        }
    }
}
