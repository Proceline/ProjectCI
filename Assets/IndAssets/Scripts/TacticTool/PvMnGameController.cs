using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvMnGameController : MonoBehaviour
    {
        [SerializeField] private FeLiteGameVisual gameVisual;

        [SerializeField]
        private InputActionReference onBattleActionConfirmed;

        [SerializeField]
        private InputActionReference onBattleActionCanceled;

        [SerializeField]
        private UnityEvent<LevelCellBase, CellState> onConfirmedAtBattlegroundWithState;

        [SerializeField]
        private UnityEvent onCanceledAtBattleground;

        [SerializeField]
        private PvSoSimpleVoidEvent onBattleStartedEvent;

        public bool IsActionLocked { get; set; }

        private void Start()
        {
            onBattleActionConfirmed.action.canceled += OnBattleActionConfirmed;
            onBattleActionCanceled.action.canceled += OnBattleActionCanceled;
            onBattleStartedEvent.RegisterCallback(EnableBasicBattleActions);
        }

        private void OnDestroy()
        {
            onBattleActionConfirmed.action.canceled -= OnBattleActionConfirmed;
            onBattleActionCanceled.action.canceled -= OnBattleActionCanceled;
            onBattleActionConfirmed.action.Disable();
            onBattleActionCanceled.action.Disable();
        }

        private void EnableBasicBattleActions()
        {
            onBattleActionConfirmed.action.Enable();
            onBattleActionCanceled.action.Enable();
        }

        private void OnBattleActionConfirmed(InputAction.CallbackContext context)
        {
            var currentHoverCell = gameVisual.CurrentHoverCell;
            if (!currentHoverCell) return;
            onConfirmedAtBattlegroundWithState?.Invoke(currentHoverCell, currentHoverCell.GetCellState());
        }

        private void OnBattleActionCanceled(InputAction.CallbackContext context)
        {
            onCanceledAtBattleground?.Invoke();
        }
    }
}