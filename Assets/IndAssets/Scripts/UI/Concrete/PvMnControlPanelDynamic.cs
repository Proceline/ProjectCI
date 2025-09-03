using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnControlPanelDynamic : PvMnControlPanel
    {

        public override List<PvMnCustomButtonSupport> ControlButtons => _controlButtons;

        /// <summary>
        /// If you assigned, you need to clean listener at last
        /// </summary>
        public override int NumOfSlots
        {
            get => _controlButtons.Count;
            set => EnableButtonsWithCount(value);
        }

        private readonly List<PvMnCustomButtonSupport> _controlButtons = new();

        [SerializeField] 
        private PvMnCustomButtonSupport controlButtonPrefab;
        
        [SerializeField]
        private UnityEvent<int> onClickSlotByDefault;

        private void DisableAllControlButtons()
        {
            _controlButtons.ForEach(button =>
            {
               button.gameObject.SetActive(false);
            });
        }

        private void EnableButtonsWithCount(int targetCount)
        {
            DisableAllControlButtons();
            for (var i = 0; i < targetCount; i++)
            {
                if (i >= _controlButtons.Count)
                {
                    var newButton = Instantiate(controlButtonPrefab, buttonsContainer);
                    newButton.ButtonIndex = i;
                    _controlButtons.Add(newButton);
                }
                else
                {
                    var button = _controlButtons[i];
                    button.gameObject.SetActive(true);
                }
            }
        }
    }
}
