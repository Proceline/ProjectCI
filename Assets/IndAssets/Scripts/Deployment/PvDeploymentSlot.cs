using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;

namespace ProjectCI.CoreSystem.Runtime.Deployment
{
    /// <summary>
    /// Deployment slot information for unit placement
    /// </summary>
    [System.Serializable]
    public class PvDeploymentSlot
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private Vector3 worldPosition;
        [SerializeField] private bool useWorldPosition = false;
        [SerializeField] private bool isAvailable = true;
        
        [System.NonSerialized]
        private PvMnSceneUnit _occupiedUnit;
        
        /// <summary>
        /// Slot index identifier
        /// </summary>
        public int SlotIndex => slotIndex;
        
        /// <summary>
        /// Grid position for this slot (if using grid-based positioning)
        /// </summary>
        public Vector2Int GridPosition => gridPosition;
        
        /// <summary>
        /// World position for this slot (if using world-based positioning)
        /// </summary>
        public Vector3 WorldPosition => worldPosition;
        
        /// <summary>
        /// Whether to use world position instead of grid position
        /// </summary>
        public bool UseWorldPosition => useWorldPosition;
        
        /// <summary>
        /// Whether this slot is available for deployment
        /// </summary>
        public bool IsAvailable => isAvailable;
        
        /// <summary>
        /// Currently occupied unit (runtime only)
        /// </summary>
        public PvMnSceneUnit OccupiedUnit
        {
            get => _occupiedUnit;
            set => _occupiedUnit = value;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public PvDeploymentSlot(int index, Vector2Int gridPos, Vector3 worldPos, bool useWorld = false)
        {
            slotIndex = index;
            gridPosition = gridPos;
            worldPosition = worldPos;
            useWorldPosition = useWorld;
            isAvailable = true;
        }
        
        /// <summary>
        /// Set slot availability
        /// </summary>
        public void SetAvailable(bool available)
        {
            isAvailable = available;
        }
        
        /// <summary>
        /// Check if slot is occupied
        /// </summary>
        public bool IsOccupied()
        {
            return _occupiedUnit != null;
        }
        
        /// <summary>
        /// Clear occupied unit
        /// </summary>
        public void ClearOccupiedUnit()
        {
            _occupiedUnit = null;
        }
    }
}
