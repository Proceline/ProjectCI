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
        private InputActionReference onAggroHintInteracted;

        [SerializeField]
        private UnityEvent<LevelCellBase, CellState> onConfirmedAtBattlegroundWithState;

        [SerializeField]
        private UnityEvent onCanceledAtBattleground;

        [SerializeField]
        private PvSoSimpleVoidEvent onBattleStartedEvent;

        [SerializeField]
        private PvSoBattleTeamEvent onRoundEndEvent;

        [SerializeField]
        private UnityEvent onAggroHintEnabled;

        [SerializeField]
        private UnityEvent onAggroHintDisabled;

        public ITeamRoundEndEvent OnRoundEndEvent => onRoundEndEvent;

        public bool IsActionLocked { get; set; }

        private void Start()
        {
            onAggroHintInteracted.action.started += OnAggroHintEnabled;
            onAggroHintInteracted.action.canceled += OnAggroHintEnabled;
            onBattleActionConfirmed.action.canceled += OnBattleActionConfirmed;
            onBattleActionCanceled.action.canceled += OnBattleActionCanceled;
            onBattleStartedEvent.RegisterCallback(EnableBasicBattleActions);
            OnRoundEndEvent.RegisterCallback(EnableBasicBattleActionAccordingToTeam);
        }

        private void OnDestroy()
        {
            onAggroHintInteracted.action.started -= OnAggroHintEnabled;
            onAggroHintInteracted.action.canceled -= OnAggroHintEnabled;
            onBattleActionConfirmed.action.canceled -= OnBattleActionConfirmed;
            onBattleActionCanceled.action.canceled -= OnBattleActionCanceled;
            onBattleActionConfirmed.action.Disable();
            onBattleActionCanceled.action.Disable();
        }

        private void EnableBasicBattleActions()
        {
            onBattleActionConfirmed.action.Enable();
            onBattleActionCanceled.action.Enable();
            onAggroHintInteracted.action.Enable();
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

        private void EnableBasicBattleActionAccordingToTeam(BattleTeam battleTeam)
        {
            if (battleTeam == BattleTeam.Friendly)
            {
                onBattleActionConfirmed.action.Disable();
                onBattleActionCanceled.action.Disable();
            }
            else if (battleTeam == BattleTeam.Hostile)
            {
                EnableBasicBattleActions();
            }
        }

        private void OnAggroHintEnabled(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    onAggroHintEnabled?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    onAggroHintDisabled?.Invoke();
                    break;
            }
        }
    }
}