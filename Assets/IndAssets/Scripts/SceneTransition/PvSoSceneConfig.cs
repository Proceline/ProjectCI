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
        [SerializeField] private Object sceneAsset;

        [Header("Prefabs to Instantiate")]
        [Tooltip("List of prefabs to instantiate when this scene is loaded")]
        [SerializeField] private List<GameObject> prefabsToInstantiate = new List<GameObject>();

        [SerializeField]
        private string sceneName;

        /// <summary>
        /// Get the scene name for runtime loading
        /// </summary>
        public string SceneName => sceneName;

        /// <summary>
        /// Get the list of prefabs to instantiate
        /// </summary>
        public List<GameObject> PrefabsToInstantiate => prefabsToInstantiate;

#if UNITY_EDITOR
        /// <summary>
        /// Get the scene asset (editor only)
        /// </summary>
        public SceneAsset SceneAsset => (SceneAsset)sceneAsset;
#endif
    }
}
