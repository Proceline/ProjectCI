using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.Saving.Data;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI
{
    /// <summary>
    /// Single character portrait item in the portrait panel
    /// </summary>
    public class PvMnCharacterPortraitItem : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private Button portraitButton;
        
        private PvCharacterSaveData _characterData;
        private UnityAction<PvCharacterSaveData> _onPortraitClicked;
        
        private void Awake()
        {
            if (portraitButton != null)
            {
                portraitButton.onClick.AddListener(OnPortraitClicked);
            }
        }
        
        private void OnDestroy()
        {
            if (portraitButton != null)
            {
                portraitButton.onClick.RemoveListener(OnPortraitClicked);
            }
        }
        
        /// <summary>
        /// Initialize portrait item with character data
        /// </summary>
        public void Initialize(PvCharacterSaveData characterData, UnityAction<PvCharacterSaveData> onPortraitClicked)
        {
            _characterData = characterData;
            _onPortraitClicked = onPortraitClicked;
            
            UpdateDisplay();
        }
        
        /// <summary>
        /// Update display with current character data
        /// </summary>
        public void UpdateDisplay()
        {
            // Empty
        }
        
        /// <summary>
        /// Set portrait sprite
        /// </summary>
        public void SetPortraitSprite(Sprite sprite)
        {
            if (portraitImage != null)
            {
                portraitImage.sprite = sprite;
            }
        }
        
        private void OnPortraitClicked()
        {
            _onPortraitClicked?.Invoke(_characterData);
        }
        
        /// <summary>
        /// Get associated character data
        /// </summary>
        public PvCharacterSaveData GetCharacterData()
        {
            return _characterData;
        }
    }
}

