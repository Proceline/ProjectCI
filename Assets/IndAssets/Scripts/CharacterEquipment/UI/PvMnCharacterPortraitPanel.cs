using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Saving;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.Saving.Data;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI
{
    /// <summary>
    /// Panel manager for character portraits on canvas
    /// </summary>
    public class PvMnCharacterPortraitPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform portraitContainer;
        [SerializeField] private PvMnCharacterPortraitItem portraitItemPrefab;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        
        private List<PvMnCharacterPortraitItem> _portraitItems = new List<PvMnCharacterPortraitItem>();
        [SerializeField] private UnityEvent<PvCharacterSaveData> onCharacterSelected;
        [SerializeField] private Toggle deployModeToggle;
        private bool _deployMode = false;

        private void OnDeployModeChanged(bool isOn)
        {
            _deployMode = isOn;
            foreach (var item in _portraitItems)
            {
                if (item != null)
                {
                    item.deployMode = _deployMode;
                }
            }
        }

        /// <summary>
        /// Initialize panel with list of character data
        /// </summary>
        public void Initialize(UnityAction<PvCharacterSaveData> onCharacterSelectedAction)
        {
            onCharacterSelected.AddListener(onCharacterSelectedAction);
            deployModeToggle.onValueChanged.AddListener(OnDeployModeChanged);
            RefreshPortraits();
        }
        
        /// <summary>
        /// Refresh all portraits
        /// </summary>
        public void RefreshPortraits()
        {
            // Clear existing portraits
            foreach (var item in _portraitItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _portraitItems.Clear();

            if (!PvSaveManager.Instance)
            {
                return;
            }
            
            // Create new portraits
            foreach (var characterData in PvSaveManager.Instance.GetUnlockedCharacters())
            {
                CreatePortraitItem(characterData);
            }

            deployModeToggle.isOn = _deployMode;
        }
        
        private void CreatePortraitItem(PvCharacterSaveData characterData)
        {
            if (portraitItemPrefab == null || portraitContainer == null)
            {
                Debug.LogWarning("PortraitItemPrefab or PortraitContainer is not set!");
                return;
            }
            
            PvMnCharacterPortraitItem itemObj = Instantiate(portraitItemPrefab, portraitContainer);
            itemObj.Initialize(characterData, OnPortraitClicked);
            _portraitItems.Add(itemObj);
        }
        
        private void OnPortraitClicked(PvCharacterSaveData characterData)
        {
            onCharacterSelected?.Invoke(characterData);
        }
        
        /// <summary>
        /// Update display for all portraits
        /// </summary>
        public void UpdateAllPortraits()
        {
            foreach (var item in _portraitItems)
            {
                if (item != null)
                {
                    item.UpdateDisplay();
                }
            }
        }
    }
}

