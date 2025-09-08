using System;
using ProjectCI.CoreSystem.Runtime.InputSupport;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [Serializable]
    public class InputActionPairForCellTarget
    {
        public InputActionReference inputActionReference;
        public UnityEvent<LevelCellBase> onCellConfirmed;

        [NonSerialized] private Action<InputAction.CallbackContext> _preloadedMethod;

        internal InputAction InputAction => inputActionReference.action;

        public void RegisterCellControl(FeLiteGameVisual visual)
        {
            UnregisterCellControl();
            _preloadedMethod = _ => onCellConfirmed?.Invoke(visual.CurrentHoverCell);
            InputAction.canceled += _preloadedMethod;
        }

        public void UnregisterCellControl()
        {
            InputAction.canceled -= _preloadedMethod;
            _preloadedMethod = null;
        }
    }
    
    [CreateAssetMenu(fileName = "New Controller", menuName = "ProjectCI Tools/MVC/Controller", order = 1)]
    public class FeLiteGameController : ScriptableObject
    {
        [SerializeField] private BattleGameRules gameRulesModel;
        [SerializeField] private FeLiteGameVisual gameVisual;
        
        [SerializeField]
        private InputActionPairForCellTarget onCellSelectedForTurnOwner;
        
        [SerializeField]
        private InputActionPairForCellTarget onCellSelectedForMovement;
        
        [SerializeField]
        private InputActionPairForCellTarget onCellSelectedWhileTargeting;

        [SerializeField] 
        private InputActionReference onControllerCancelled;

        [SerializeField]
        private UnityEvent onControllerCancelledUnityEvent;
        
        public void RegisterControlActions()
        {
            onCellSelectedForTurnOwner.RegisterCellControl(gameVisual);
            onCellSelectedForMovement.RegisterCellControl(gameVisual);
            onCellSelectedWhileTargeting.RegisterCellControl(gameVisual);
            onControllerCancelled.action.canceled += ApplyCancelFromUnityEvent;
        }

        public void UnregisterControlActions()
        {
            onCellSelectedForTurnOwner.UnregisterCellControl();
            onCellSelectedForMovement.UnregisterCellControl();
            onCellSelectedWhileTargeting.UnregisterCellControl();
            onControllerCancelled.action.canceled -= ApplyCancelFromUnityEvent;
        }

        public void DisableAllConfirm()
        {
            onCellSelectedForTurnOwner.InputAction.Disable();
            onCellSelectedForMovement.InputAction.Disable();
            onCellSelectedWhileTargeting.InputAction.Disable();
        }

        public void EnableConfirmActionOnAbilityDetermining()
        {
            EnableConfirmActionByState(UnitBattleState.UsingAbility);
        }

        public void ToggleSelectedUnitCancelAction(bool enabled)
        {
            if (enabled)
            {
                Debug.Log("Hint: ControlCancel for SelectedUnit ENABLED!");
                onControllerCancelled.action.Enable();
            }
            else
            {
                Debug.Log("Hint: ControlCancel for SelectedUnit DISABLED!");
                onControllerCancelled.action.Disable();
            }
        }

        public void SwitchEnabledConfirmAction(PvMnBattleGeneralUnit unit, UnitBattleState state)
        {
            EnableConfirmActionByState(state);
        }

        private void ApplyCancelFromUnityEvent(InputAction.CallbackContext context)
        {
            onControllerCancelledUnityEvent.Invoke();
        }
        
        private void EnableConfirmActionByState(UnitBattleState state)
        {
            DisableAllConfirm();
            switch (state)
            {
                case UnitBattleState.Moving:
                    onCellSelectedForMovement.InputAction.Enable();
                    ToggleSelectedUnitCancelAction(true);
                    break;
                case UnitBattleState.UsingAbility:
                case UnitBattleState.AbilityTargeting:
                    onCellSelectedWhileTargeting.InputAction.Enable();
                    break;
                case UnitBattleState.Finished:
                    onCellSelectedForTurnOwner.InputAction.Enable();
                    break;
                case UnitBattleState.MovingProgress:
                case UnitBattleState.Idle:
                case UnitBattleState.AbilityConfirming:
                default:
                    // Empty
                    break;
            }
        }
    }
}