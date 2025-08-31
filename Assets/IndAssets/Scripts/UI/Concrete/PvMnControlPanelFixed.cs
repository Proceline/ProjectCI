using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnControlPanelFixed : PvMnControlPanel
    {
        public override List<PvMnCustomButtonSupport> ControlButtons => fixedControlButtons;

        public override int NumOfSlots
        {
            get => fixedControlButtons.Count;
            set => Debug.LogWarning(
                $"Warning: No effect on ControlPanelType<{nameof(PvMnControlPanelFixed)}, with value {value.ToString()}>");
        }

        [SerializeField] 
        private List<PvMnCustomButtonSupport> fixedControlButtons = new();
        
        public override void ActivatePanel()
        {
            // Empty
        }
    }
}