using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private string loadingSceneName;
        public string LoadingSceneName => loadingSceneName;

        [Header("Scene Configurations")]
        [Tooltip("List of all scene configurations")]
        [SerializeField] private List<PvSoSceneConfig> sceneConfigs = new List<PvSoSceneConfig>();

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
    }
}
