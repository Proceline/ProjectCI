using TMPro;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.CharacterEquipment.UI
{
    /// <summary>
    /// Tooltip window for displaying weapon/relic information on hover
    /// </summary>
    public class PvMnEquipStatusTip : MonoBehaviour
    {
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI basicInfoTip;
        
        private void Awake()
        {
            if (tooltipPanel)
            {
                tooltipPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Show tooltip with weapon/relic information
        /// </summary>
        public void ShowTooltip(string instanceId, string characterId, bool enabled)
        {
            tooltipPanel.SetActive(enabled);
            if (!enabled) return;

            if (string.IsNullOrEmpty(characterId))
            {
                basicInfoTip.text = "[No one Equip this]";
            }
            else
            {
                basicInfoTip.text = $"<color=green>{characterId}</color> [is Equipping this]";
            }
        }
    }
}

