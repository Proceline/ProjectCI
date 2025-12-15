using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace ProjectCI.CoreSystem.Runtime.SceneTransition
{
    /// <summary>
    /// Runtime manager for scene transitions with loading screen
    /// Handles scene loading, prefab instantiation, and loading progress display
    /// </summary>
    public partial class PvSoSceneManagerConfig
    {
        private bool _isTransitioning = false;
        private string _targetSceneName;
        [NonSerialized] private PvSoSceneConfig _targetSceneConfig;

        /// <summary>
        /// Transition to a scene by scene name
        /// </summary>
        public async void TransitionToScene(string sceneName)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning($"Scene transition already in progress. Cannot transition to {sceneName}");
                return;
            }

            _targetSceneConfig = GetSceneConfig(sceneName);
            if (_targetSceneConfig == null)
            {
                Debug.LogError($"Scene config not found for scene: {sceneName}");
                return;
            }

            _targetSceneName = sceneName;
            await TransitSceneAsync();
        }

        /// <summary>
        /// Transition to a scene by scene config
        /// </summary>
        public async void TransitionToScene(PvSoSceneConfig sceneConfig)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning($"Scene transition already in progress. Cannot transition to {sceneConfig.SceneName}");
                return;
            }

            if (sceneConfig == null)
            {
                Debug.LogError("Scene config is null!");
                return;
            }

            _targetSceneConfig = sceneConfig;
            _targetSceneName = sceneConfig.SceneName;
            await TransitSceneAsync();
        }

        /// <summary>
        /// Main transition coroutine
        /// </summary>
        private async Awaitable TransitSceneAsync()
        {
            _isTransitioning = true;

            // Step 1: Load loading scene
            string loadingSceneName = LoadingSceneName;
            if (string.IsNullOrEmpty(loadingSceneName))
            {
                Debug.LogError("Loading scene name is empty!");
                _isTransitioning = false;
                return;
            }

            // Load loading scene
            await SceneManager.LoadSceneAsync(loadingSceneName);

            var textHint = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            Debug.LogError(textHint);

            // Step 2: Load target scene asynchronously
            AsyncOperation loadTargetScene = SceneManager.LoadSceneAsync(_targetSceneName, LoadSceneMode.Additive);
            loadTargetScene.allowSceneActivation = false;

            // Wait until scene is ready (90% loaded)
            while (loadTargetScene.progress < 0.9f)
            {
                if (textHint.Length > 0)
                {
                    UpdateLoadingProgress(textHint[0], loadTargetScene.progress);
                }

                await Awaitable.EndOfFrameAsync();
            }

            // Allow scene activation
            loadTargetScene.allowSceneActivation = true;

            // Wait for scene to be fully loaded
            while (!loadTargetScene.isDone)
            {
                if (textHint.Length > 0)
                {
                    UpdateLoadingProgress(textHint[0], loadTargetScene.progress);
                }
                await Awaitable.EndOfFrameAsync();
            }

            // Step 3: Instantiate prefabs after scene is loaded
            await Awaitable.EndOfFrameAsync();

            if (_targetSceneConfig != null && _targetSceneConfig.PrefabsToInstantiate != null)
            {
                foreach (var prefab in _targetSceneConfig.PrefabsToInstantiate)
                {
                    if (prefab != null)
                    {
                        Instantiate(prefab);
                    }
                }
            }

            // Update to 100%
            if (textHint.Length > 0)
            {
                UpdateLoadingProgress(textHint[0], 1);
            }
            
            await SceneManager.UnloadSceneAsync(loadingSceneName);
            
            _isTransitioning = false;
        }
        
        private void UpdateLoadingProgress(TextMeshProUGUI loadingText, float progress)
        {
            // Clamp progress between 0 and 1
            progress = Mathf.Clamp01(progress);

            // Convert to percentage (0-100)
            int percentage = Mathf.RoundToInt(progress * 100f);

            // Update text if available
            if (loadingText != null)
            {
                loadingText.text = $"{percentage}%";
            }
        }

        /// <summary>
        /// Check if a transition is currently in progress
        /// </summary>
        public bool IsTransitioning => _isTransitioning;
    }
}
