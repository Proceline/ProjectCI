using System.Threading.Tasks;
using ProjectCI.CoreSystem.Runtime.Saving.Data;

namespace ProjectCI.CoreSystem.Runtime.Saving.Interfaces
{
    /// <summary>
    /// Interface for save/load system implementations
    /// Supports both local and cloud storage backends
    /// </summary>
    public interface IPvSaveSystem
    {
        /// <summary>
        /// Check if save system is initialized
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Initialize the save system
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// Save game data
        /// </summary>
        /// <param name="saveData">Data to save</param>
        /// <param name="slotName">Save slot name (optional, for multiple saves)</param>
        /// <returns>True if save was successful</returns>
        Task<bool> SaveAsync(PvSaveData saveData, string slotName = "default");
        
        /// <summary>
        /// Load game data
        /// </summary>
        /// <param name="slotName">Save slot name (optional, for multiple saves)</param>
        /// <returns>Loaded save data, or null if load failed</returns>
        Task<PvSaveData> LoadAsync(string slotName = "default");
        
        /// <summary>
        /// Check if a save slot exists
        /// </summary>
        /// <param name="slotName">Save slot name</param>
        /// <returns>True if slot exists</returns>
        Task<bool> SaveExistsAsync(string slotName = "default");
        
        /// <summary>
        /// Delete a save slot
        /// </summary>
        /// <param name="slotName">Save slot name</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteSaveAsync(string slotName = "default");
        
        /// <summary>
        /// Get all available save slot names
        /// </summary>
        /// <returns>Array of save slot names</returns>
        Task<string[]> GetSaveSlotsAsync();
        
        /// <summary>
        /// Cleanup resources
        /// </summary>
        void Cleanup();
    }
}

