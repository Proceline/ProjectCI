using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.Deployment
{
    /// <summary>
    /// Deployment controller that manages which units are deployed and their positions
    /// </summary>
    [CreateAssetMenu(fileName = "NewDeploymentController", menuName = "ProjectCI/Deployment/Deployment Controller", order = 1)]
    public class PvSoDeploymentController : ScriptableObject
    {
        [SerializeField]
        private InputActionReference onDeployCellFocus;

        [Header("Deployment Configuration")]
        [SerializeField] private PvSoLevelData levelData;

        [NonSerialized]
        private readonly List<GameObject> _loadedMarkers = new();

        /// <summary>
        /// Get level data
        /// </summary>
        public PvSoLevelData LevelData => levelData;

        /// <summary>
        /// Used in DeploymentBridge UnityEvent
        /// </summary>
        /// <param name="markerPrefab"></param>
        public void ShowAllSpawnHints(GameObject markerPrefab)
        {
            onDeployCellFocus.action.Enable();

            foreach (var slotPosition in levelData.FriendlySlots)
            {
                var marker = Instantiate(markerPrefab, slotPosition, Quaternion.identity);
                _loadedMarkers.Add(marker);
            }
        }

        public void RemoveAllSpawnHints()
        {
            onDeployCellFocus.action.Disable();

            foreach (var marker in _loadedMarkers)
            {
                Destroy(marker);
            }
        }

        public void PutCameraOnPositionOnStarted()
        {
            levelData.PutCameraOnPosition();
        }
    }
}
