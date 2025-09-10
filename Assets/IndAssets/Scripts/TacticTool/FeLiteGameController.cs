using System;
using ProjectCI.CoreSystem.Runtime.InputSupport;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    
    [CreateAssetMenu(fileName = "New Controller", menuName = "ProjectCI Tools/MVC/Controller", order = 1)]
    public class FeLiteGameController : ScriptableObject
    {
        /// <summary>
        /// Internal Cell Target Pair
        /// </summary>
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
                _preloadedMethod = _ =>
                {
                    if (!_isBattlegroundInteractionOn)
                        return;
                    onCellConfirmed?.Invoke(visual.CurrentHoverCell);
                };
                InputAction.canceled += _preloadedMethod;
            }

            public void UnregisterCellControl()
            {
                InputAction.canceled -= _preloadedMethod;
                _preloadedMethod = null;
            }
        }
        
        [SerializeField] private BattleGameRules gameRulesModel;
        [SerializeField] private FeLiteGameVisual gameVisual;
        
        [SerializeField]
        private InputActionPairForCellTarget onCellSelectedForTurnOwner;
        
        [SerializeField]
        private InputActionPairForCellTarget onCellSelectedForMovement;
        
        [SerializeField]
        private InputActionPairForCellTarget onCellSelectedWhileTargeting;
        
        [SerializeField] 
        private InputActionReference onActionSelectionCanceled;
        
        [SerializeField] 
        private InputActionReference onUnitSelectionCanceled;
        
        [SerializeField] 
        private UnityEvent onActionSelectionCanceledBindingEvent;
        
        [SerializeField] 
        private UnityEvent onUnitSelectionCanceledBindingEvent;

        [Header("UI Controllers"), SerializeField]
        private InputActionReference onBattleControlPanelCanceled;

        internal InputAction OnBattleControlPanelCanceledAction => onBattleControlPanelCanceled.ToInputAction();

        private static bool _isBattlegroundInteractionOn = true;
        public bool IsBattlegroundInteractionOn
        {
            get => _isBattlegroundInteractionOn;
            set => _isBattlegroundInteractionOn = value;
        }
        
        public void RegisterControlActions()
        {
            onCellSelectedForTurnOwner.RegisterCellControl(gameVisual);
            onCellSelectedForMovement.RegisterCellControl(gameVisual);
            onCellSelectedWhileTargeting.RegisterCellControl(gameVisual);
            onActionSelectionCanceled.action.canceled += ApplyActionCancelFromUnityEvent;
            onUnitSelectionCanceled.action.canceled += ApplyUnitCancelFromUnityEvent;
        }

        public void UnregisterControlActions()
        {
            onCellSelectedForTurnOwner.UnregisterCellControl();
            onCellSelectedForMovement.UnregisterCellControl();
            onCellSelectedWhileTargeting.UnregisterCellControl();
            onActionSelectionCanceled.action.canceled -= ApplyActionCancelFromUnityEvent;
            onUnitSelectionCanceled.action.canceled -= ApplyUnitCancelFromUnityEvent;
        }

        private void DisableAllConfirm()
        {
            onCellSelectedForTurnOwner.InputAction.Disable();
            onCellSelectedForMovement.InputAction.Disable();
            onCellSelectedWhileTargeting.InputAction.Disable();
        }

        /// <summary>
        /// This normally controlled by Control Panel/Finished
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleSelectedUnitCancelAction(bool enabled)
        {
            if (enabled)
            {
                Debug.Log("<color=blue>Cancel Action Enabled!</color>");
                onActionSelectionCanceled.action.Enable();
            }
            else
            {
                Debug.Log("<color=red>Cancel Action Disabled!</color>");
                onActionSelectionCanceled.action.Disable();
            }
        }
        
        private void TogglePendingSelectUnitAction(bool enabled)
        {
            if (enabled)
            {
                Debug.Log("<color=blue>Cancel Selected Unit Enabled!</color>");
                onUnitSelectionCanceled.action.Enable();
            }
            else
            {
                Debug.Log("<color=red>Cancel Selected Unit Enabled!</color>");
                onUnitSelectionCanceled.action.Disable();
            }
        }

        /// <summary>
        /// This event will be applied before EnableConfirmActionByState
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="stateEventParam"></param>
        public void SwitchPendingSelectUnitActionWhileStating(IEventOwner owner, UnitStateEventParam stateEventParam)
        {
            var state = stateEventParam.battleState;
            var stateBehaviour = stateEventParam.behaviour;

            if (state == UnitBattleState.Moving && stateBehaviour == UnitStateBehaviour.Adding)
            {
                TogglePendingSelectUnitAction(true);
                ToggleSelectedUnitCancelAction(false);
            }
            else if (state == UnitBattleState.UsingAbility)
            {
                switch (stateBehaviour)
                {
                    case UnitStateBehaviour.Adding:
                        ToggleSelectedUnitCancelAction(true);
                        break;
                    case UnitStateBehaviour.Popping:
                        ToggleSelectedUnitCancelAction(false);
                        Debug.Log(
                            $"<color=yellow>State <{UnitBattleState.MovingProgress.ToString()}> will be SKIPPED during Cancelling</color>");
                        TogglePendingSelectUnitAction(true);
                        break;
                    default:
                        throw new IndexOutOfRangeException(
                            $"Using Behaviour<{stateBehaviour.ToString()}> is NOT allowed during <{state.ToString()}>");
                }
            }
            else if (state == UnitBattleState.Finished)
            {
                if (stateBehaviour != UnitStateBehaviour.Adding && stateBehaviour != UnitStateBehaviour.Emphasis &&
                    stateBehaviour != UnitStateBehaviour.Clear)
                {
                    throw new IndexOutOfRangeException(
                        $"Using Behaviour<{stateBehaviour.ToString()}> is NOT allowed during <{state.ToString()}>");
                }

                TogglePendingSelectUnitAction(false);
                ToggleSelectedUnitCancelAction(false);
            }
            else if (stateBehaviour != UnitStateBehaviour.Emphasis)
            {
                Debug.Log(
                    $"<color=red>Apply Disable for Cancel Selected Unit during <{state.ToString()}> with behaviour<{stateBehaviour.ToString()}>!</color>");
                TogglePendingSelectUnitAction(false);
            }
        }

        public void SwitchEnabledConfirmAction(PvMnBattleGeneralUnit unit, UnitBattleState state)
        {
            EnableConfirmActionByState(state);
        }

        private void ApplyActionCancelFromUnityEvent(InputAction.CallbackContext context)
        {
            onActionSelectionCanceledBindingEvent.Invoke();
        }
        
        private void ApplyUnitCancelFromUnityEvent(InputAction.CallbackContext context)
        {
            onUnitSelectionCanceledBindingEvent.Invoke();
        }
        
        private void EnableConfirmActionByState(UnitBattleState state)
        {
            DisableAllConfirm();
            switch (state)
            {
                case UnitBattleState.Moving:
                    onCellSelectedForMovement.InputAction.Enable();
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