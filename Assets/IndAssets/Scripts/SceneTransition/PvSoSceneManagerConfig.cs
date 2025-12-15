using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectCI.CoreSystem.Runtime.SceneTransition
{
    /// <summary>
    /// Main configuration for scene transition manager
    /// Contains loading scene reference and all scene configurations
    /// </summary>
    [CreateAssetMenu(fileName = "SceneManagerConfig", menuName = "ProjectCI/Scene Transition/Scene Manager Config", order = 0)]
    public partial class PvSoSceneManagerConfig : ScriptableObject
    {
        [Header("Loading Scene")]
        [Tooltip("The loading scene that will be shown during scene transitions")]
#if UNITY_EDITOR
        [SerializeField] private SceneAsset loadingSceneAsset;
#else
        [SerializeField] private Object loadingSceneAsset; // Dummy field for runtime
#endif

        [Header("Scene Configurations")]
        [Tooltip("List of all scene configurations")]
        [SerializeField] private List<PvSoSceneConfig> sceneConfigs = new List<PvSoSceneConfig>();

        /// <summary>
        /// Get the loading scene name for runtime loading
        /// </summary>
        public string LoadingSceneName
        {
            get
            {
#if UNITY_EDITOR
                if (loadingSceneAsset != null)
                {
                    return System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(loadingSceneAsset));
                }
#endif
                return string.Empty;
            }
        }

        /// <summary>
        /// Get all scene configurations
        /// </summary>
        public List<PvSoSceneConfig> SceneConfigs => sceneConfigs;

        /// <summary>
        /// Get scene config by scene name
        /// </summary>
        public PvSoSceneConfig GetSceneConfig(string sceneName)
        {
            foreach (var config in sceneConfigs)
            {
                if (config != null && config.SceneName == sceneName)
                {
                    return config;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Get the loading scene asset (editor only)
        /// </summary>
        public SceneAsset LoadingSceneAsset => loadingSceneAsset;
#endif
    }
}
