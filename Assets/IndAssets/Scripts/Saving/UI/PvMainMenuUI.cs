using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.Saving.Data;
using TMPro;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace ProjectCI.CoreSystem.Runtime.Saving.UI
{
    /// <summary>
    /// Main menu UI component with three buttons: New Game, Continue Last Game, Load Game
    /// </summary>
    public class PvMainMenuUI : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button newGameButton; // Start new game button
        [SerializeField] private Button continueGameButton; // Continue last game button
        [SerializeField] private Button loadGameButton; // Load game button
        
        [Header("UI References")]
        [SerializeField] private PvSaveListUI saveListUI; // Save list UI component
        [SerializeField] private GameObject loadingOverlay; // Black overlay to block UI during loading
        [SerializeField] private TextMeshProUGUI loadingText; // Optional loading text
        
        [Header("New Game Dialog")]
        [SerializeField] private GameObject newGameDialog; // Dialog panel for new game input
        [SerializeField] private TMP_InputField saveNameInputField; // Input field for save name
        [SerializeField] private Button confirmNewGameButton; // Confirm button in dialog
        [SerializeField] private Button cancelNewGameButton; // Cancel button in dialog
        [SerializeField] private TextMeshProUGUI dialogErrorText; // Optional error text in dialog
        
        [Header("New Game Settings")]
        [SerializeField] private string defaultNewGameSlotName = "default"; // Default slot name for new game
        
        private bool _isRefreshing = false; // Track if we're currently refreshing save list

        [SerializeField] private UnityEvent<List<PvSaveDetails>> onSaveDataListRefreshRequested;
        [NonSerialized] private PvSaveDetails _lastUsedSave;
        
        private void Start()
        {
            SetupButtons();
            InitializeUI();
        }
        
        private void OnEnable()
        {
            PvSaveManager.onAllSaveDataListed += OnSaveDataListRefreshed;
        }
        
        private void OnDisable()
        {
            PvSaveManager.onAllSaveDataListed -= OnSaveDataListRefreshed;
        }
        
        /// <summary>
        /// Setup button callbacks
        /// </summary>
        private void SetupButtons()
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveAllListeners();
                newGameButton.onClick.AddListener(OnNewGameClicked);
            }
            
            if (continueGameButton != null)
            {
                continueGameButton.onClick.RemoveAllListeners();
                continueGameButton.onClick.AddListener(OnContinueGameClicked);
            }
            
            if (loadGameButton != null)
            {
                loadGameButton.onClick.RemoveAllListeners();
                loadGameButton.onClick.AddListener(OnLoadGameClicked);
            }
            
            // Setup new game dialog buttons
            if (confirmNewGameButton != null)
            {
                confirmNewGameButton.onClick.RemoveAllListeners();
                confirmNewGameButton.onClick.AddListener(OnConfirmNewGameClicked);
            }
            
            if (cancelNewGameButton != null)
            {
                cancelNewGameButton.onClick.RemoveAllListeners();
                cancelNewGameButton.onClick.AddListener(OnCancelNewGameClicked);
            }
        }
        
        /// <summary>
        /// Initialize UI state
        /// </summary>
        private void InitializeUI()
        {
            // Hide save list UI initially
            if (saveListUI != null)
            {
                saveListUI.gameObject.SetActive(false);
            }
            
            // Hide loading overlay initially
            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(false);
            }
            
            // Hide new game dialog initially
            if (newGameDialog != null)
            {
                newGameDialog.SetActive(false);
            }
            
            // Hide error text initially
            if (dialogErrorText != null)
            {
                dialogErrorText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Handle new game button click - show dialog for save name input
        /// </summary>
        private void OnNewGameClicked()
        {
            if (!PvSaveManager.Instance)
            {
                Debug.LogError("PvSaveManager instance not found");
                return;
            }
            
            if (!PvSaveManager.Instance.IsInitialized)
            {
                Debug.LogWarning("Save manager not initialized yet");
                return;
            }
            
            // Show new game dialog
            ShowNewGameDialog();
        }
        
        /// <summary>
        /// Show new game dialog
        /// </summary>
        private void ShowNewGameDialog()
        {
            if (newGameDialog != null)
            {
                newGameDialog.SetActive(true);
            }
            
            // Set default save name in input field
            if (saveNameInputField != null)
            {
                saveNameInputField.text = defaultNewGameSlotName;
                saveNameInputField.Select();
                saveNameInputField.ActivateInputField();
                
                // Setup Enter key to confirm
                saveNameInputField.onSubmit.RemoveAllListeners();
                saveNameInputField.onSubmit.AddListener((text) => OnConfirmNewGameClicked());
            }
            
            // Hide error text
            if (dialogErrorText != null)
            {
                dialogErrorText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Handle confirm new game button click - create new game with input name
        /// </summary>
        private async void OnConfirmNewGameClicked()
        {
            if (!PvSaveManager.Instance)
            {
                Debug.LogError("PvSaveManager instance not found");
                return;
            }
            
            // Get save name from input field
            string saveName = GetSaveNameFromInput();
            
            if (string.IsNullOrWhiteSpace(saveName))
            {
                ShowDialogError("Save name cannot be empty");
                return;
            }
            
            // Validate save name (remove invalid characters)
            saveName = SanitizeSaveName(saveName);
            
            if (string.IsNullOrWhiteSpace(saveName))
            {
                ShowDialogError("Invalid save name");
                return;
            }
            
            // Check if save name already exists by checking save list
            bool saveExists = await CheckSaveNameExists(saveName);
            if (saveExists)
            {
                ShowDialogError($"Save name '{saveName}' already exists. Please choose a different name.");
                return;
            }
            
            // Hide dialog
            HideNewGameDialog();
            
            // Create new save data
            var saveManager = PvSaveManager.Instance;
            await saveManager.CreateNewGameAsync(saveName);
            
            // Save the new game
            bool success = await saveManager.SaveGameAsync(saveName);
            
            if (success)
            {
                Debug.Log($"New game created successfully: {saveName}");
                // You can add scene transition logic here
                // For example: SceneManager.LoadScene("GameScene");
            }
            else
            {
                Debug.LogError($"Failed to create new game: {saveName}");
                // Show dialog again with error
                ShowNewGameDialog();
                ShowDialogError("Failed to create new game. Please try again.");
            }
        }
        
        /// <summary>
        /// Handle cancel new game button click - hide dialog
        /// </summary>
        private void OnCancelNewGameClicked()
        {
            HideNewGameDialog();
        }
        
        /// <summary>
        /// Hide new game dialog
        /// </summary>
        private void HideNewGameDialog()
        {
            if (newGameDialog != null)
            {
                newGameDialog.SetActive(false);
            }
            
            // Clear input field
            if (saveNameInputField != null)
            {
                saveNameInputField.text = "";
            }
            
            // Hide error text
            if (dialogErrorText != null)
            {
                dialogErrorText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Get save name from input field
        /// </summary>
        private string GetSaveNameFromInput()
        {
            if (saveNameInputField != null)
            {
                return saveNameInputField.text?.Trim();
            }
            return defaultNewGameSlotName;
        }
        
        /// <summary>
        /// Sanitize save name by removing invalid characters
        /// </summary>
        private string SanitizeSaveName(string saveName)
        {
            if (string.IsNullOrWhiteSpace(saveName))
            {
                return "";
            }
            
            // Remove invalid file name characters
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                saveName = saveName.Replace(c.ToString(), "");
            }
            
            // Trim and return
            return saveName.Trim();
        }
        
        /// <summary>
        /// Show error message in dialog
        /// </summary>
        private void ShowDialogError(string errorMessage)
        {
            if (dialogErrorText != null)
            {
                dialogErrorText.text = errorMessage;
                dialogErrorText.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"New Game Dialog Error: {errorMessage}");
            }
        }
        
        /// <summary>
        /// Handle continue game button click
        /// </summary>
        private async void OnContinueGameClicked()
        {
            if (!PvSaveManager.Instance)
            {
                Debug.LogError("PvSaveManager instance not found");
                return;
            }
            
            if (!PvSaveManager.Instance.IsInitialized)
            {
                Debug.LogWarning("Save manager not initialized yet");
                return;
            }
            
            if (_lastUsedSave == null)
            {
                Debug.LogWarning("No save file found to continue");
                // You can show a message to the user here
                return;
            }
            
            // Load the last used save
            bool success = await PvSaveManager.Instance.LoadGameByGuidAsync(_lastUsedSave.SaveFolderGuid);
            
            if (success)
            {
                Debug.Log($"Continued game: {_lastUsedSave.SaveSlotName}");
                // You can add scene transition logic here
                // For example: SceneManager.LoadScene("GameScene");
            }
            else
            {
                Debug.LogError($"Failed to continue game: {_lastUsedSave.SaveSlotName}");
            }
        }
        
        /// <summary>
        /// Handle load game button click
        /// </summary>
        private void OnLoadGameClicked()
        {
            if (!PvSaveManager.Instance)
            {
                Debug.LogError("PvSaveManager instance not found");
                return;
            }
            
            if (!PvSaveManager.Instance.IsInitialized)
            {
                Debug.LogWarning("Save manager not initialized yet");
                return;
            }
            
            // Start refreshing save list
            StartRefreshSaveList();
        }
        
        /// <summary>
        /// Start refreshing save list with loading overlay
        /// </summary>
        private async void StartRefreshSaveList()
        {
            if (_isRefreshing || !PvSaveManager.Instance)
            {
                Debug.LogWarning("Save list is already being refreshed");
                return;
            }
            
            _isRefreshing = true;
            
            // Show loading overlay
            ShowLoadingOverlay(true);
            
            // Hide save list UI
            if (saveListUI)
            {
                saveListUI.gameObject.SetActive(false);
            }
            
            await PvSaveManager.Instance.InitializeSaveDataList();
            
            _isRefreshing = false;
            ShowLoadingOverlay(false);
            if (saveListUI)
            {
                saveListUI.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// Called when save data list refresh is complete
        /// </summary>
        private void OnSaveDataListRefreshed(List<PvSaveDetails> saveDetailsList)
        {
            // Find the save with the most recent SaveTime
            PvSaveDetails lastUsedSave = null;
            DateTime latestTime = DateTime.MinValue;
            
            foreach (var saveDetails in saveDetailsList)
            {
                DateTime saveTime;
                if (DateTime.TryParse(saveDetails.SaveTime, out saveTime))
                {
                    if (saveTime > latestTime)
                    {
                        latestTime = saveTime;
                        lastUsedSave = saveDetails;
                    }
                }
            }

            if (continueGameButton)
            {
                continueGameButton.interactable = lastUsedSave != null;
                _lastUsedSave = lastUsedSave;
            }
            
            Debug.Log($"Save list refreshed. Found {saveDetailsList?.Count ?? 0} save(s)");
            onSaveDataListRefreshRequested?.Invoke(saveDetailsList);
        }
        
        /// <summary>
        /// Show or hide loading overlay
        /// </summary>
        private void ShowLoadingOverlay(bool show)
        {
            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(show);
            }
            
            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(show);
                if (show)
                {
                    loadingText.text = "Loading saves...";
                }
            }
        }
        
        /// <summary>
        /// Check if a save name already exists
        /// </summary>
        private async System.Threading.Tasks.Task<bool> CheckSaveNameExists(string saveName)
        {
            if (!PvSaveManager.Instance || !PvSaveManager.Instance.IsInitialized)
            {
                return false;
            }
            
            try
            {
                // Use reflection to access the private _saveSystem field
                var saveSystemField = typeof(PvSaveManager)
                    .GetField("_saveSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (saveSystemField == null)
                {
                    Debug.LogWarning("Could not access save system to check save existence");
                    return false;
                }
                
                var saveSystem = saveSystemField.GetValue(PvSaveManager.Instance);
                if (saveSystem == null)
                {
                    return false;
                }
                
                // Call SaveExistsAsync using reflection
                var saveExistsMethod = saveSystem.GetType().GetMethod("SaveExistsAsync");
                if (saveExistsMethod == null)
                {
                    Debug.LogWarning("SaveExistsAsync method not found");
                    return false;
                }
                
                var task = saveExistsMethod.Invoke(saveSystem, new object[] { saveName }) as System.Threading.Tasks.Task<bool>;
                if (task != null)
                {
                    return await task;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error checking save existence: {ex.Message}");
            }
            
            return false;
        }
        
        /// <summary>
        /// Hide save list UI and return to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (saveListUI != null)
            {
                saveListUI.gameObject.SetActive(false);
            }
        }
    }
}
