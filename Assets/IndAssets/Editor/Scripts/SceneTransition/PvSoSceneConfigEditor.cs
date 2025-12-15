using UnityEditor;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.SceneTransition;

namespace ProjectCI.CoreSystem.Editor.SceneTransition
{
    /// <summary>
    /// Custom editor for PvSoSceneConfig
    /// Provides a clean interface for configuring scene transitions
    /// </summary>
    [CustomEditor(typeof(PvSoSceneConfig))]
    public class PvSoSceneConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _sceneAssetProperty;
        private SerializedProperty _prefabsToInstantiateProperty;

        private void OnEnable()
        {
            _sceneAssetProperty = serializedObject.FindProperty("sceneAsset");
            _prefabsToInstantiateProperty = serializedObject.FindProperty("prefabsToInstantiate");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Scene Asset field
            EditorGUILayout.PropertyField(_sceneAssetProperty, new GUIContent("Scene Asset", "The scene asset to load"));

            // Display scene name (read-only)
            PvSoSceneConfig config = (PvSoSceneConfig)target;
            if (config.SceneAsset != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Scene Name:", GUILayout.Width(100));
                EditorGUILayout.LabelField(config.SceneName, EditorStyles.helpBox);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Please assign a Scene Asset", MessageType.Warning);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);

            // Prefabs to instantiate
            EditorGUILayout.LabelField($"Prefabs to Instantiate ({_prefabsToInstantiateProperty.arraySize})", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_prefabsToInstantiateProperty, new GUIContent("Prefabs", "List of prefabs to instantiate when this scene is loaded"), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
