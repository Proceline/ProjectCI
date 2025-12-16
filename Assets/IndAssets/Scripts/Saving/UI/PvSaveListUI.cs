using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Saving.Data;
using TMPro;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Saving.UI
{
    /// <summary>
    /// UI component for displaying and managing save slots in the load menu
    /// </summary>
    public class PvSaveListUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform saveSlotContainer; // Container for save slot UI items
        [SerializeField] private PvSaveSlotUI saveSlotPrefab; // Prefab for individual save slot UI
        
        [Header("Empty State")]
        [SerializeField] private GameObject emptyStatePanel; // Panel shown when no saves exist
        [SerializeField] private TextMeshProUGUI emptyStateText; // Text for empty state
        
        private List<PvSaveSlotUI> _saveSlotUIs = new List<PvSaveSlotUI>();
        [SerializeField] private UnityEvent<PvSaveDetails> onLoadedGameConfirmed;
        
        /// <summary>
        /// Update the UI with the list of save slots
        /// </summary>
        public void UpdateSaveList(List<PvSaveDetails> saveDetailsList)
        {
            // Clear existing UI items
            ClearSaveSlots();
            
            // Show empty state if no saves
            if (saveDetailsList == null || saveDetailsList.Count == 0)
            {
                ShowEmptyState(true);
                return;
            }
            
            ShowEmptyState(false);
            
            // Create UI for each save slot
            foreach (var saveDetails in saveDetailsList)
            {
                CreateSaveSlotUI(saveDetails);
            }
        }
        
        /// <summary>
        /// Create a UI element for a save slot
        /// </summary>
        private void CreateSaveSlotUI(PvSaveDetails saveDetails)
        {
            if (saveSlotContainer == null)
            {
                Debug.LogError("Save slot container is not assigned");
                return;
            }
            
            // Get or add PvSaveSlotUI component
            var saveSlotUI = Instantiate(saveSlotPrefab, saveSlotContainer);
            
            saveSlotUI.Initialize(saveDetails, OnSaveSlotSelected);
            
            _saveSlotUIs.Add(saveSlotUI);
        }
        
        /// <summary>
        /// Clear all save slot UI elements
        /// </summary>
        private void ClearSaveSlots()
        {
            foreach (var slotUI in _saveSlotUIs)
            {
                if (slotUI != null && slotUI.gameObject != null)
                {
                    Destroy(slotUI.gameObject);
                }
            }
            _saveSlotUIs.Clear();
        }
        
        /// <summary>
        /// Show or hide the empty state panel
        /// </summary>
        private void ShowEmptyState(bool show)
        {
            if (emptyStatePanel != null)
            {
                emptyStatePanel.SetActive(show);
            }
            
            if (emptyStateText != null && show)
            {
                emptyStateText.text = "No save files found";
            }
        }
        
        /// <summary>
        /// Called when a save slot is selected
        /// </summary>
        private void OnSaveSlotSelected(PvSaveDetails saveDetails)
        {
            if (PvSaveManager.Instance == null)
            {
                Debug.LogError("PvSaveManager instance not found");
                return;
            }
            
            onLoadedGameConfirmed.Invoke(saveDetails);
        }
    }
}
