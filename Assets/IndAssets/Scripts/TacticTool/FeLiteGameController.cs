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
    public class FeLiteGameController : ScriptableObject, IGameController
    {
        [SerializeField] private BattleGameRules gameRulesModel;
        [SerializeField] private FeLiteGameVisual gameVisual;
        
        [SerializeField]
        private InputActionManager inputActionManager;

        [SerializeField]
        private InputActionPairForCellTarget onCellSelectedFoTurnOwner;
        
        [SerializeField]
        private InputActionPairForCellTarget onCellSelectedForMovement;
        
        [SerializeField]
        private InputActionPairForCellTarget onCellSelectedWhileTargeting;
        
        public void RegisterControlActions()
        {
            onCellSelectedFoTurnOwner.RegisterCellControl(gameVisual);
            onCellSelectedForMovement.RegisterCellControl(gameVisual);
        }

        public void UnregisterControlActions()
        {
            onCellSelectedFoTurnOwner.UnregisterCellControl();
            onCellSelectedForMovement.UnregisterCellControl();
        }

        private void DisableAllConfirm()
        {
            onCellSelectedFoTurnOwner.InputAction.Disable();
            onCellSelectedForMovement.InputAction.Disable();
            onCellSelectedWhileTargeting.InputAction.Disable();
        }

        public void SwitchEnabledConfirmAction(PvMnBattleGeneralUnit unit, UnitBattleState state)
        {
            DisableAllConfirm();
            switch (state)
            {
                case UnitBattleState.Moving:
                    onCellSelectedForMovement.InputAction.Enable();
                    break;
                case UnitBattleState.UsingAbility:
                    onCellSelectedWhileTargeting.InputAction.Enable();
                    break;
                case UnitBattleState.Finished:
                    onCellSelectedFoTurnOwner.InputAction.Enable();
                    break;
                case UnitBattleState.MovingProgress:
                case UnitBattleState.AbilityTargeting:
                case UnitBattleState.Idle:
                case UnitBattleState.AbilityConfirming:
                default:
                    // Empty
                    break;
            }
        }
    }
}