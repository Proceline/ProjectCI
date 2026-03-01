using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;
using System;
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
        private InputActionReference onAggroHintToggled;

        [SerializeField]
        private InputActionReference onCursorPositionTracked;
        private InputAction _cursorPositionAction;

        [SerializeField]
        private Camera cursorCamera;

        [SerializeField]
        private LayerMask cellUsingLayer;

        public static bool IsAggroHintToggled { get; private set; }

        [SerializeField]
        private PvSoTurnViewEndEvent onTurnLockerEvent;

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

        [SerializeField]
        private UnityEvent<LevelCellBase> onCellRayHit;

        [SerializeField]
        private UnityEvent<LevelCellBase> onCellRayMissed;

        private LevelCellBase HitBufferedCell { get; set; }

        public ITeamRoundEndEvent OnRoundEndEvent => onRoundEndEvent;

        private bool IsTeamInputLocked { get; set; }

        private bool IsFriendlyLockerEnabled { get; set; }

        public static bool IsControllerDisabled { get; set; }

        private void Start()
        {
            onCursorPositionTracked.action.Enable();
            _cursorPositionAction = onCursorPositionTracked.ToInputAction();

            onAggroHintInteracted.action.started += OnAggroHintEnabled;
            onAggroHintInteracted.action.canceled += OnAggroHintEnabled;
            onAggroHintToggled.action.canceled += OnAggroHintToggling;

            onBattleActionConfirmed.action.canceled += OnBattleActionConfirmed;
            onBattleActionCanceled.action.canceled += OnBattleActionCanceled;

            onBattleStartedEvent.RegisterCallback(EnableBasicBattleActions);
            OnRoundEndEvent.RegisterCallback(EnableBasicBattleActionAccordingToTeam);
            onTurnLockerEvent.RegisterCallback(OnRaiserLockerApplied);
        }

        private void OnDestroy()
        {
            onCursorPositionTracked.action.Disable();

            onAggroHintInteracted.action.started -= OnAggroHintEnabled;
            onAggroHintInteracted.action.canceled -= OnAggroHintEnabled;
            onAggroHintToggled.action.canceled -= OnAggroHintToggling;

            onBattleActionConfirmed.action.canceled -= OnBattleActionConfirmed;
            onBattleActionCanceled.action.canceled -= OnBattleActionCanceled;
            DisableBasicBattleActions();

            onBattleStartedEvent.UnregisterCallback(EnableBasicBattleActions);
            OnRoundEndEvent.UnregisterCallback(EnableBasicBattleActionAccordingToTeam);
            onTurnLockerEvent.UnregisterCallback(OnRaiserLockerApplied);
        }

        private void EnableBasicBattleActions()
        {
            onBattleActionConfirmed.action.Enable();
            onBattleActionCanceled.action.Enable();
            onAggroHintInteracted.action.Enable();
            onAggroHintToggled.action.Enable();
        }

        private void DisableBasicBattleActions()
        {
            IsAggroHintToggled = false;
            onAggroHintDisabled?.Invoke();

            DisableBasicBattleActionsOnly();
        }

        private void DisableBasicBattleActionsOnly()
        {
            onBattleActionConfirmed.action.Disable();
            onBattleActionCanceled.action.Disable();
            onAggroHintInteracted.action.Disable();
            onAggroHintToggled.action.Disable();
        }

        private void Update()
        {
            if (IsControllerDisabled || IsTeamInputLocked || IsFriendlyLockerEnabled)
            {
                ClearHitBufferCell();
                return;
            }

            var ray = cursorCamera.ScreenPointToRay(_cursorPositionAction.ReadValue<Vector2>());
            if (Physics.Raycast(ray, out var hit, 1000f, cellUsingLayer))
            {
                if (hit.collider.TryGetComponent<PvMnLevelCell>(out var cell))
                {
                    if (cell != HitBufferedCell)
                    {
                        HitBufferedCell = cell;
                        onCellRayHit.Invoke(cell);
                    }
                }
                else if (HitBufferedCell)
                {
                    onCellRayMissed.Invoke(HitBufferedCell);
                }
            }
            else
            {
                ClearHitBufferCell();
            }
        }

        private void ClearHitBufferCell()
        {
            if (HitBufferedCell)
            {
                onCellRayMissed.Invoke(HitBufferedCell);
                HitBufferedCell = null;
            }
        }

        private void OnBattleActionConfirmed(InputAction.CallbackContext context)
        {
            if (IsControllerDisabled || IsFriendlyLockerEnabled)
            {
                return;
            }

            var currentHoverCell = gameVisual.CurrentHoverCell;
            if (!currentHoverCell) return;
            onConfirmedAtBattlegroundWithState?.Invoke(currentHoverCell, currentHoverCell.GetCellState());
        }

        private void OnBattleActionCanceled(InputAction.CallbackContext context)
        {
            if (IsControllerDisabled)
            {
                return;
            }

            onCanceledAtBattleground?.Invoke();

            if (IsAggroHintToggled)
            {
                onAggroHintDisabled?.Invoke();
                onAggroHintEnabled?.Invoke();
            }
        }

        private void EnableBasicBattleActionAccordingToTeam(BattleTeam battleTeam)
        {
            if (battleTeam == BattleTeam.Friendly)
            {
                DisableBasicBattleActions();
                IsTeamInputLocked = true;
            }
            else if (battleTeam == BattleTeam.Hostile)
            {
                EnableBasicBattleActions();
                IsTeamInputLocked = false;
            }
        }

        private void OnAggroHintEnabled(InputAction.CallbackContext context)
        {
            if (IsControllerDisabled)
            {
                return;
            }

            switch (context.phase)
            {
                case InputActionPhase.Started:
                    IsAggroHintToggled = true;
                    onAggroHintEnabled?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    IsAggroHintToggled = false;
                    onAggroHintDisabled?.Invoke();
                    break;
            }
        }

        private void OnAggroHintToggling(InputAction.CallbackContext context)
        {
            if (IsControllerDisabled)
            {
                return;
            }

            IsAggroHintToggled = !IsAggroHintToggled;

            if (IsAggroHintToggled)
            {
                onAggroHintEnabled?.Invoke();
            }
            else
            {
                onAggroHintDisabled?.Invoke();
            }
        }

        private void OnRaiserLockerApplied(bool isLocked)
        {
            IsFriendlyLockerEnabled = isLocked;
            if (isLocked)
            {
                onAggroHintDisabled?.Invoke();
                DisableBasicBattleActionsOnly();

                ClearHitBufferCell();
            }
            else if (!IsTeamInputLocked)
            {
                if (IsAggroHintToggled)
                {
                    onAggroHintEnabled?.Invoke();
                }

                EnableBasicBattleActions();
            }
        }
    }
}