using System;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.Saving.Data;
using TMPro;

namespace ProjectCI.CoreSystem.Runtime.Saving.UI
{
    /// <summary>
    /// UI component for displaying a single save slot
    /// </summary>
    public class PvSaveSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI slotNameText;
        [SerializeField] private TextMeshProUGUI saveTimeText;
        [SerializeField] private TextMeshProUGUI playTimeText;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button yesButton; // Yes button for delete confirmation
        [SerializeField] private Button noButton; // No button for delete confirmation
        
        private PvSaveDetails _saveDetails;
        private Action<PvSaveDetails> _onSelected;
        
        /// <summary>
        /// Initialize the save slot UI with save details
        /// </summary>
        /// <param name="saveDetails">The save details to display</param>
        /// <param name="onSelected">Callback when this slot is selected</param>
        public void Initialize(PvSaveDetails saveDetails, Action<PvSaveDetails> onSelected)
        {
            _saveDetails = saveDetails;
            _onSelected = onSelected;
            
            UpdateUI();
            SetupButtons();
        }
        
        /// <summary>
        /// Update the UI elements with save information
        /// </summary>
        private void UpdateUI()
        {
            if (_saveDetails == null)
            {
                return;
            }
            
            // Update slot name
            if (slotNameText != null)
            {
                slotNameText.text = _saveDetails.SaveSlotName;
            }
            
            // Update save time
            if (saveTimeText != null)
            {
                if (DateTime.TryParse(_saveDetails.SaveTime, out DateTime saveTime))
                {
                    // Format: "Last saved: MM/dd/yyyy HH:mm"
                    saveTimeText.text = $"Last saved: {saveTime:MM/dd/yyyy HH:mm}";
                }
                else
                {
                    saveTimeText.text = $"Last saved: {_saveDetails.SaveTime}";
                }
            }
            
            // Update play time
            if (playTimeText != null)
            {
                TimeSpan playTimeSpan = TimeSpan.FromSeconds(_saveDetails.TotalPlayTime);
                playTimeText.text = $"Play time: {playTimeSpan.Hours:D2}:{playTimeSpan.Minutes:D2}:{playTimeSpan.Seconds:D2}";
            }
        }
        
        /// <summary>
        /// Setup button callbacks
        /// </summary>
        private void SetupButtons()
        {
            // Setup load button
            if (loadButton != null)
            {
                loadButton.onClick.RemoveAllListeners();
                loadButton.onClick.AddListener(() => _onSelected?.Invoke(_saveDetails));
            }
            
            // If no load button, use the main button component
            if (loadButton == null)
            {
                var button = GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => _onSelected?.Invoke(_saveDetails));
                }
            }
            
            // Setup delete button
            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(OnDeleteClicked);
            }
            
            // Setup yes button
            if (yesButton != null)
            {
                yesButton.onClick.RemoveAllListeners();
                yesButton.onClick.AddListener(OnYesClicked);
            }
            
            // Setup no button
            if (noButton != null)
            {
                noButton.onClick.RemoveAllListeners();
                noButton.onClick.AddListener(OnNoClicked);
            }
            
            // Initialize button visibility state
            ResetDeleteButton();
        }
        
        /// <summary>
        /// Handle delete button click - show confirmation buttons
        /// </summary>
        private void OnDeleteClicked()
        {
            if (_saveDetails == null)
            {
                return;
            }
            
            // Show confirmation (Yes/No buttons)
            ShowDeleteConfirmation();
        }
        
        /// <summary>
        /// Handle yes button click - confirm deletion
        /// </summary>
        private async void OnYesClicked()
        {
            await ConfirmDelete();
        }
        
        /// <summary>
        /// Handle no button click - cancel deletion
        /// </summary>
        private void OnNoClicked()
        {
            CancelDelete();
        }
        
        /// <summary>
        /// Show delete confirmation UI (hide delete button, show yes/no buttons)
        /// </summary>
        private void ShowDeleteConfirmation()
        {
            // Hide delete button
            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(false);
            }
            
            // Show yes and no buttons
            if (yesButton != null)
            {
                yesButton.gameObject.SetActive(true);
            }
            
            if (noButton != null)
            {
                noButton.gameObject.SetActive(true);
            }
            
            // Auto-cancel after 5 seconds if no action
            CancelDeleteAfterDelay();
        }
        
        /// <summary>
        /// Cancel delete confirmation after a delay if user doesn't click
        /// </summary>
        private void CancelDeleteAfterDelay()
        {
            // Cancel after 5 seconds if no action
            CancelInvoke(nameof(ResetDeleteButton));
            Invoke(nameof(ResetDeleteButton), 5f);
        }
        
        /// <summary>
        /// Reset delete button to normal state (show delete button, hide yes/no buttons)
        /// </summary>
        private void ResetDeleteButton()
        {
            // Show delete button
            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(true);
            }
            
            // Hide yes and no buttons
            if (yesButton != null)
            {
                yesButton.gameObject.SetActive(false);
            }
            
            if (noButton != null)
            {
                noButton.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Confirm and execute delete operation
        /// </summary>
        private async Awaitable ConfirmDelete()
        {
            CancelInvoke(nameof(ResetDeleteButton));
            
            if (!PvSaveManager.Instance)
            {
                Debug.LogError("PvSaveManager instance not found");
                ResetDeleteButton();
                return;
            }
            
            // Get the save system to delete by GUID
            // Since DeleteSaveAsync uses slot name, we need to use slot name
            bool success = await DeleteSaveBySlotName(_saveDetails.SaveSlotName);
            
            if (success)
            {
                Debug.Log($"Save deleted: {_saveDetails.SaveSlotName}");
                
                await PvSaveManager.Instance.InitializeSaveDataList();
            }
            else
            {
                Debug.LogError($"Failed to delete save: {_saveDetails.SaveSlotName}");
                ResetDeleteButton();
            }
        }
        
        /// <summary>
        /// Delete save by slot name using reflection to access the save system
        /// </summary>
        private async System.Threading.Tasks.Task<bool> DeleteSaveBySlotName(string slotName)
        {
            var saveManager = ProjectCI.CoreSystem.Runtime.Saving.PvSaveManager.Instance;
            if (saveManager == null)
            {
                return false;
            }
            
            // Use reflection to access the private _saveSystem field
            var saveSystemField = typeof(ProjectCI.CoreSystem.Runtime.Saving.PvSaveManager)
                .GetField("_saveSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (saveSystemField == null)
            {
                Debug.LogError("Could not access save system");
                return false;
            }
            
            var saveSystem = saveSystemField.GetValue(saveManager);
            if (saveSystem == null)
            {
                Debug.LogError("Save system is null");
                return false;
            }
            
            // Call DeleteSaveAsync using reflection
            var deleteMethod = saveSystem.GetType().GetMethod("DeleteSaveAsync");
            if (deleteMethod == null)
            {
                Debug.LogError("DeleteSaveAsync method not found");
                return false;
            }
            
            try
            {
                var task = deleteMethod.Invoke(saveSystem, new object[] { slotName }) as System.Threading.Tasks.Task<bool>;
                if (task != null)
                {
                    return await task;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error deleting save: {ex.Message}");
            }
            
            return false;
        }
        
        /// <summary>
        /// Cancel delete confirmation
        /// </summary>
        public void CancelDelete()
        {
            CancelInvoke(nameof(ResetDeleteButton));
            ResetDeleteButton();
        }
        
        /// <summary>
        /// Get the save details for this slot
        /// </summary>
        public PvSaveDetails GetSaveDetails()
        {
            return _saveDetails;
        }
        
        private void OnDestroy()
        {
            // Cancel any pending invokes
            CancelInvoke();
        }
    }
}
