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
        [Header("Friendly Deployment Slots")]
        [SerializeField] private List<PvDeploymentSlot> friendlySlots = new List<PvDeploymentSlot>();
        
        [Header("Hostile Deployment Slots")]
        [SerializeField] private List<PvDeploymentSlot> hostileSlots = new List<PvDeploymentSlot>();
        
        /// <summary>
        /// Get all friendly deployment slots
        /// </summary>
        public List<PvDeploymentSlot> FriendlySlots => friendlySlots;
        
        /// <summary>
        /// Get all hostile deployment slots
        /// </summary>
        public List<PvDeploymentSlot> HostileSlots => hostileSlots;
        
        /// <summary>
        /// Get friendly slot by index
        /// </summary>
        public PvDeploymentSlot GetFriendlySlot(int index)
        {
            if (index >= 0 && index < friendlySlots.Count)
            {
                return friendlySlots[index];
            }
            return null;
        }
        
        /// <summary>
        /// Get hostile slot by index
        /// </summary>
        public PvDeploymentSlot GetHostileSlot(int index)
        {
            if (index >= 0 && index < hostileSlots.Count)
            {
                return hostileSlots[index];
            }
            return null;
        }
        
        /// <summary>
        /// Get available friendly slot
        /// </summary>
        public PvDeploymentSlot GetAvailableFriendlySlot()
        {
            foreach (var slot in friendlySlots)
            {
                if (slot.IsAvailable && !slot.IsOccupied())
                {
                    return slot;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Get available hostile slot
        /// </summary>
        public PvDeploymentSlot GetAvailableHostileSlot()
        {
            foreach (var slot in hostileSlots)
            {
                if (slot.IsAvailable && !slot.IsOccupied())
                {
                    return slot;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Get slot by index for a specific team
        /// </summary>
        public PvDeploymentSlot GetSlotByIndex(int index, bool isFriendly)
        {
            return isFriendly ? GetFriendlySlot(index) : GetHostileSlot(index);
        }
    }
}
