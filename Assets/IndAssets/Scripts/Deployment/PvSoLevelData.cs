using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Deployment
{
    /// <summary>
    /// Level data containing deployment slot information for friendly and hostile units
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "ProjectCI/Deployment/Level Data", order = 1)]
    public class PvSoLevelData : ScriptableObject
    {
        [SerializeField] private Vector3 scanStartPivot;
        public int gridWidth;
        public int gridHeight;

        [SerializeField] private Vector3 cameraPosition;

        [Header("Friendly Deployment Slots")]
        [SerializeField] private List<Vector3> friendlySlots = new List<Vector3>();

        public Vector3 LevelStartPosition => scanStartPivot;

        /// <summary>
        /// Get all friendly deployment slots
        /// </summary>
        public List<Vector3> FriendlySlots => friendlySlots;

        public void PutCameraOnPosition()
        {
            var mainCamera = Camera.main;
            var cameraParent = mainCamera.transform.parent;
            var targetRoot = mainCamera.transform;
            while (cameraParent != null)
            {
                targetRoot = cameraParent;
                cameraParent = cameraParent.parent;
            }

            targetRoot.position = cameraPosition;
        }
    }
}
