using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectCI.CoreSystem.Runtime.SceneTransition
{
    /// <summary>
    /// Configuration for a single scene transition
    /// Contains scene asset reference and prefabs to instantiate when loading
    /// </summary>
    [CreateAssetMenu(fileName = "NewSceneConfig", menuName = "ProjectCI/Scene Transition/Scene Config", order = 1)]
    public class PvSoSceneConfig : ScriptableObject
    {
        [Header("Scene Configuration")]
        [Tooltip("The scene asset to load")]
#if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset;
#else
        [SerializeField] private Object sceneAsset; // Dummy field for runtime
#endif

        [Header("Prefabs to Instantiate")]
        [Tooltip("List of prefabs to instantiate when this scene is loaded")]
        [SerializeField] private List<GameObject> prefabsToInstantiate = new List<GameObject>();

        /// <summary>
        /// Get the scene name for runtime loading
        /// </summary>
        public string SceneName
        {
            get
            {
#if UNITY_EDITOR
                if (sceneAsset != null)
                {
                    return System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sceneAsset));
                }
#endif
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the list of prefabs to instantiate
        /// </summary>
        public List<GameObject> PrefabsToInstantiate => prefabsToInstantiate;

#if UNITY_EDITOR
        /// <summary>
        /// Get the scene asset (editor only)
        /// </summary>
        public SceneAsset SceneAsset => sceneAsset;
#endif
    }
}
