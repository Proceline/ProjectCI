using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnGameControllerBridge : MonoBehaviour
    {
        [SerializeField] private FeLiteGameController controller;
        [SerializeField] private UnityEvent onControlPanelUiCanceledApplied;
        private Action<InputAction.CallbackContext> _recordedOnControlPanelUiCanceledApplied;

        private void Start()
        {
            _recordedOnControlPanelUiCanceledApplied = _ => onControlPanelUiCanceledApplied.Invoke();
            controller.OnBattleControlPanelCanceledAction.canceled += _recordedOnControlPanelUiCanceledApplied;
        }

        private void OnDisable()
        {
            controller.OnBattleControlPanelCanceledAction.canceled -= _recordedOnControlPanelUiCanceledApplied;
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        public void ToggleControlPanelUICancel(bool isOn)
        {
            controller.ToggleSelectedUnitCancelAction(!isOn);
            if (isOn)
            {
                controller.OnBattleControlPanelCanceledAction.Enable();
            }
            else
            {
                controller.OnBattleControlPanelCanceledAction.Disable();
            }
        }
    }
}