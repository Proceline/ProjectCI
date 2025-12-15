using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace ProjectCI.CoreSystem.Runtime.SceneTransition
{
    /// <summary>
    /// Runtime manager for scene transitions with loading screen
    /// Handles scene loading, prefab instantiation, and loading progress display
    /// </summary>
    public class PvSceneTransitionManager : MonoBehaviour
    {
        private static PvSceneTransitionManager _instance;
        public static PvSceneTransitionManager Instance => _instance;

        [Header("Configuration")]
        [SerializeField] private PvSoSceneManagerConfig config;

        [Header("Loading UI References")]
        [Tooltip("Text component to display loading percentage (e.g., '100%')")]
        [SerializeField] private TextMeshProUGUI loadingPercentageText;

        [Tooltip("Optional: GameObject to show/hide during loading")]
        [SerializeField] private GameObject loadingUI;

        private bool _isTransitioning = false;
        private string _targetSceneName;
        private PvSoSceneConfig _targetSceneConfig;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Transition to a scene by scene name
        /// </summary>
        public void TransitionToScene(string sceneName)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning($"Scene transition already in progress. Cannot transition to {sceneName}");
                return;
            }

            if (config == null)
            {
                Debug.LogError("SceneManagerConfig is not assigned!");
                return;
            }

            _targetSceneConfig = config.GetSceneConfig(sceneName);
            if (_targetSceneConfig == null)
            {
                Debug.LogError($"Scene config not found for scene: {sceneName}");
                return;
            }

            _targetSceneName = sceneName;
            StartCoroutine(TransitionCoroutine());
        }

        /// <summary>
        /// Transition to a scene by scene config
        /// </summary>
        public void TransitionToScene(PvSoSceneConfig sceneConfig)
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
            StartCoroutine(TransitionCoroutine());
        }

        /// <summary>
        /// Main transition coroutine
        /// </summary>
        private IEnumerator TransitionCoroutine()
        {
            _isTransitioning = true;

            // Step 1: Load loading scene
            string loadingSceneName = config.LoadingSceneName;
            if (string.IsNullOrEmpty(loadingSceneName))
            {
                Debug.LogError("Loading scene name is empty!");
                _isTransitioning = false;
                yield break;
            }

            // Show loading UI if available
            if (loadingUI != null)
            {
                loadingUI.SetActive(true);
            }

            // Load loading scene
            AsyncOperation loadLoadingScene = SceneManager.LoadSceneAsync(loadingSceneName);
            loadLoadingScene.allowSceneActivation = true;

            while (!loadLoadingScene.isDone)
            {
                UpdateLoadingProgress(loadLoadingScene.progress);
                yield return null;
            }

            // Step 2: Load target scene asynchronously
            AsyncOperation loadTargetScene = SceneManager.LoadSceneAsync(_targetSceneName);
            loadTargetScene.allowSceneActivation = false;

            // Wait until scene is ready (90% loaded)
            while (loadTargetScene.progress < 0.9f)
            {
                UpdateLoadingProgress(loadTargetScene.progress);
                yield return null;
            }

            // Allow scene activation
            loadTargetScene.allowSceneActivation = true;

            // Wait for scene to be fully loaded
            while (!loadTargetScene.isDone)
            {
                UpdateLoadingProgress(loadTargetScene.progress);
                yield return null;
            }

            // Step 3: Instantiate prefabs after scene is loaded
            yield return new WaitForEndOfFrame(); // Wait one frame to ensure scene is fully initialized

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
            UpdateLoadingProgress(1.0f);

            // Hide loading UI if available
            if (loadingUI != null)
            {
                loadingUI.SetActive(false);
            }

            _isTransitioning = false;
        }

        /// <summary>
        /// Update loading progress display
        /// </summary>
        private void UpdateLoadingProgress(float progress)
        {
            // Clamp progress between 0 and 1
            progress = Mathf.Clamp01(progress);

            // Convert to percentage (0-100)
            int percentage = Mathf.RoundToInt(progress * 100f);

            // Update text if available
            if (loadingPercentageText != null)
            {
                loadingPercentageText.text = $"{percentage}%";
            }
        }

        /// <summary>
        /// Check if a transition is currently in progress
        /// </summary>
        public bool IsTransitioning => _isTransitioning;
    }
}
