using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    }
}