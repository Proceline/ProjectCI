using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.CharacterEquipment.Data;

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
        private List<PvCharacterEquipmentData> _characterDataList = new List<PvCharacterEquipmentData>();
        private System.Action<PvCharacterEquipmentData> _onCharacterSelected;
        
        /// <summary>
        /// Initialize panel with list of character data
        /// </summary>
        public void Initialize(List<PvCharacterEquipmentData> characterDataList, System.Action<PvCharacterEquipmentData> onCharacterSelected)
        {
            _characterDataList = characterDataList ?? new List<PvCharacterEquipmentData>();
            _onCharacterSelected = onCharacterSelected;
            
            RefreshPortraits();
        }
        
        /// <summary>
        /// Add a character to the panel
        /// </summary>
        public void AddCharacter(PvCharacterEquipmentData characterData)
        {
            if (characterData == null) return;
            
            if (!_characterDataList.Contains(characterData))
            {
                _characterDataList.Add(characterData);
            }
            
            CreatePortraitItem(characterData);
        }
        
        /// <summary>
        /// Remove a character from the panel
        /// </summary>
        public void RemoveCharacter(PvCharacterEquipmentData characterData)
        {
            if (characterData == null) return;
            
            _characterDataList.Remove(characterData);
            
            var itemToRemove = _portraitItems.Find(item => item.GetCharacterData() == characterData);
            if (itemToRemove != null)
            {
                _portraitItems.Remove(itemToRemove);
                Destroy(itemToRemove.gameObject);
            }
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
            
            // Create new portraits
            foreach (var characterData in _characterDataList)
            {
                CreatePortraitItem(characterData);
            }
        }
        
        private void CreatePortraitItem(PvCharacterEquipmentData characterData)
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
        
        private void OnPortraitClicked(PvCharacterEquipmentData characterData)
        {
            _onCharacterSelected?.Invoke(characterData);
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

