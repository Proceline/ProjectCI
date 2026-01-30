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

        [SerializeField]
        private PvSoBattleTeamEvent onRoundEndEvent;

        public ITeamRoundEndEvent OnRoundEndEvent => onRoundEndEvent;

        public bool IsActionLocked { get; set; }

        private void Start()
        {
            onBattleActionConfirmed.action.canceled += OnBattleActionConfirmed;
            onBattleActionCanceled.action.canceled += OnBattleActionCanceled;
            onBattleStartedEvent.RegisterCallback(EnableBasicBattleActions);
            OnRoundEndEvent.RegisterCallback(EnableBasicBattleActionAccordingToTeam);
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
    }
}