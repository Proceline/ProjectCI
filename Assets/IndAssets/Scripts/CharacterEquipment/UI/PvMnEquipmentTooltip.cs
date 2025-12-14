using TMPro;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI
{
    /// <summary>
    /// Tooltip window for displaying weapon/relic information on hover
    /// </summary>
    public class PvMnEquipmentTooltip : MonoBehaviour
    {
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        private RectTransform _rectTransform;
        private Canvas _parentCanvas;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();
            
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Show tooltip with weapon/relic information
        /// </summary>
        public void ShowTooltip(string name, string description, Vector3 position)
        {
            if (tooltipPanel == null) return;
            
            if (nameText != null)
            {
                nameText.text = name;
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = description;
            }
            
            tooltipPanel.SetActive(true);
            
            // Position tooltip near cursor/mouse position
            if (_parentCanvas != null && _rectTransform != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentCanvas.transform as RectTransform,
                    position,
                    _parentCanvas.worldCamera,
                    out Vector2 localPoint);
                
                _rectTransform.anchoredPosition = localPoint;
            }
        }
        
        /// <summary>
        /// Hide tooltip
        /// </summary>
        public void HideTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Update tooltip position (for following mouse)
        /// </summary>
        public void UpdatePosition(Vector3 position)
        {
            if (!tooltipPanel.activeSelf || _parentCanvas == null || _rectTransform == null)
                return;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas.transform as RectTransform,
                position,
                _parentCanvas.worldCamera,
                out Vector2 localPoint);
            
            _rectTransform.anchoredPosition = localPoint;
        }
    }
}

