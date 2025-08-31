using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnControlPanelDynamic : PvMnControlPanel
    {

        public override List<PvMnCustomButtonSupport> ControlButtons => _controlButtons;

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

        public override void ActivatePanel()
        {
            // Empty
        }

        private void DisableAllControlButtons()
        {
            _controlButtons.ForEach(button =>
            {
               button.Button.onClick.RemoveAllListeners();
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
                    _controlButtons.Add(newButton);
                }
                else
                {
                    var indexOfButton = i;
                    var button = _controlButtons[indexOfButton];
                    button.gameObject.SetActive(true);
                    button.Button.onClick.AddListener(() => onClickSlotByDefault?.Invoke(indexOfButton));
                }
            }
        }
    }
}
