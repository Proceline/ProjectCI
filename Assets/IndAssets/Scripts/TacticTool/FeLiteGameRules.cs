using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using System;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "NewGameRules", menuName = "ProjectCI Tools/GameRules/Create FeLiteGameRules", order = 1)]
    public partial class FeLiteGameRules : BattleGameRules
    {
        [SerializeField]
        private PvSoUnitAbility[] allAbilities;
        private readonly Dictionary<string, PvMnBattleGeneralUnit> _unitIdToBattleUnitHash = new();
        private readonly Dictionary<string, PvSoUnitAbility> _abilityIdToAbilityHash = new();

        [NonSerialized] private PvMnBattleGeneralUnit _selectedUnit;

        [SerializeField]
        private LayerMask[] layerMasksRuleList;

        [Header("Update Support")]
        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onUpdateSupport;

        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onUpdateSupportWithAbility;

        [Header("Select Support Event View-Only")]

        /// <summary>
        /// View-only Event Raiser while Turn Owner Selected
        /// </summary>
        [SerializeField]
        private PvSoUnitSelectEvent raiserOwnerSelectedViewEvent;

        /// <summary>
        /// View-only Response while Turn Owner Selected
        /// </summary>
        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onTurnOwnerSelectedPreview;

        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onTurnOwnerDeSelectedPreview;

        [Header("On Turn Support"), SerializeField]
        private PvSoTurnViewEndEvent raiserTurnLockerEvent;

        [Header("State Support")]

        [SerializeField]
        private PvSoUnitBattleStateEvent raiserOnStateChangedBeforeUEvent;

        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit, UnitBattleState> onStateChangedViewSupport;


        public UnitBattleState CurrentBattleState =>
            _selectedUnit ? _selectedUnit.GetCurrentState() : UnitBattleState.Finished;

        #region Injected Fields

        public static PvSoSimpleDamageApplyEvent XRaiserSimpleDamageApplyEvent
        {
            get
            {
                try
                {
                    return RaiserSimpleDamageApplyEvent;
                }
                catch
                {
                    throw new NullReferenceException("ERROR: FeLiteGameRules didn't involved injection!");
                }
            }
        }

        [Inject] private static readonly PvSoSimpleDamageApplyEvent RaiserSimpleDamageApplyEvent;
        [Inject] private static readonly IUnitPrepareEvent RaiserManualFinishOrRestPrepareEvent;
        [Inject] private static readonly ITeamRoundEndEvent RaiserTeamRoundEndEvent;

        [Inject] private static readonly IUnitGeneralCombatingEvent RaiserOnCombatingListCreatedEvent;
        [Inject] private static readonly IUnitCombatingQueryEndEvent RaiserOnCombatingQueryEndEvent;

        [Inject] private static readonly ICombatingTurnEndEvent RaiserCombatingTurnEndLogically;

        [Inject] private static readonly IAnimationOutLengthFunc GetPresetAnimationLengthFunc;
        [Inject] private static readonly IAnimationOutBreakPointFunc GetPresetAnimationBreakPointFunc;
        [Inject] private static readonly PvSoAnimationTriggerEvent RaiserAnimationPlayEvent;

        #endregion

        protected override void StartGame()
        {
            _unitIdToBattleUnitHash.Clear();
            _abilityIdToAbilityHash.Clear();

            CurrentTeam = BattleTeam.Friendly;

            foreach (var singleAbility in allAbilities)
            {
                if (string.IsNullOrEmpty(singleAbility.ID))
                {
                    singleAbility.GenerateNewID();
                }
                _abilityIdToAbilityHash.TryAdd(singleAbility.ID, singleAbility);
            }

            var units = FindObjectsByType<PvMnBattleGeneralUnit>(FindObjectsSortMode.None);
            foreach (var unit in units)
            {
                _unitIdToBattleUnitHash.TryAdd(unit.ID, unit);
            }

            TacticBattleManager.HandleGameStarted();

            BeginTeamTurn(CurrentTeam);
        }

        #region StateSwitchAndActions

        public void ChangeStateToConfirmedTargeting()
        {
            if (!_selectedUnit)
            {
                throw new NullReferenceException("ERROR: No selected unit while Selecting ACTION OPTION!");
            }
            ChangeStateForSelectedUnit(UnitBattleState.AbilityTargeting);
        }

        private void ChangeStateForSelectedUnit(UnitBattleState state)
        {
            var stateBehaviour = UnitStateBehaviour.Emphasis;
            var selectedUnit = _selectedUnit;
            if (selectedUnit)
            {
                if (state != UnitBattleState.Finished)
                {
                    selectedUnit.AddState(state);
                    stateBehaviour = UnitStateBehaviour.Adding;
                }
                else
                {
                    selectedUnit.ClearStates();
                    stateBehaviour = UnitStateBehaviour.Clear;
                }
            }

            raiserOnStateChangedBeforeUEvent.Raise(selectedUnit, state, stateBehaviour);

            if (CurrentTeam == BattleTeam.Friendly)
            {
                onStateChangedViewSupport?.Invoke(selectedUnit, state);
            }
        }

        private UnitBattleState CancelStatePurelyForUnit(PvMnBattleGeneralUnit unit, UnitBattleState stateToBeRemoved)
        {
            unit.RemoveLastState();
            var stateAfterRemove = unit.GetCurrentState();
            if (stateAfterRemove == UnitBattleState.MovingProgress)
            {
                unit.RemoveLastState();
                stateAfterRemove = unit.GetCurrentState();
            }
            // Notify which state is going to be removed
            raiserOnStateChangedBeforeUEvent.Raise(unit, stateToBeRemoved, UnitStateBehaviour.Popping);
            // Notify which state is ON

            onStateChangedViewSupport?.Invoke(unit, stateAfterRemove);
            return stateAfterRemove;
        }

        /// <summary>
        /// Select Turn Owner Unit state will be different from other State Switch
        /// </summary>
        /// <param name="selectingUnit"></param>
        /// <exception cref="Exception"></exception>
        private void PushStateAfterSelectUnit(PvMnBattleGeneralUnit selectingUnit)
        {
            if (_selectedUnit)
            {
                throw new Exception("ERROR: Must deselect unit first to Select next Unit!");
            }

            if (selectingUnit.IsMoving())
            {
                Debug.LogWarning("Warning: Selected Unit is moving!");
                return;
            }

            PushStateAfterSelectUnitLogicWithoutView(selectingUnit);

            onTurnOwnerSelectedPreview?.Invoke(selectingUnit);
            raiserOwnerSelectedViewEvent.Raise(selectingUnit, UnitSelectBehaviour.Select);
        }

        /// <summary>
        /// Push State After Select Unit Logic Without Selecting View Response,
        /// Used in AI Turn, AI Bridge Mono Component
        /// </summary>
        /// <param name="selectingUnit"></param>
        public void PushStateAfterSelectUnitLogicWithoutView(PvMnBattleGeneralUnit selectingUnit)
        {
            _selectedUnit = selectingUnit;
            _selectedUnit.BindToOnMovementPostCompleted(UpdatePlayerStateAfterRegularMove);

            ChangeStateForSelectedUnit(_selectedUnit.GetCurrentMovementPoints() > 0
                ? UnitBattleState.Moving
                : UnitBattleState.AbilityTargeting);
        }

        public void ClearStateAndDeselectUnit()
        {
            ChangeStateForSelectedUnit(UnitBattleState.Finished);   // Clean up all States
            if (!_selectedUnit)
            {
                return;
            }

            _selectedUnit.UnBindFromOnMovementPostCompleted(UpdatePlayerStateAfterRegularMove);
            onTurnOwnerDeSelectedPreview?.Invoke(_selectedUnit);
            raiserOwnerSelectedViewEvent.Raise(_selectedUnit, UnitSelectBehaviour.Deselect);
            _selectedUnit = null;
        }

        /// <summary>
        /// Only Used for binding to OnMovementEnd of Pawn
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void UpdatePlayerStateAfterRegularMove()
        {
            if (!_selectedUnit) return;
            if (_selectedUnit.GetCurrentState() != UnitBattleState.MovingProgress)
            {
                throw new Exception(
                    $"State ERROR: Current State must be <{UnitBattleState.MovingProgress.ToString()}>, but Having <{CurrentBattleState.ToString()}>");
            }

            if (_selectedUnit.GetCurrentActionPoints() > 0)
            {
                ChangeStateForSelectedUnit(UnitBattleState.AbilityTargeting);//UsingAbility);
            }
            else
            {
                ArchiveUnitBehaviourPoints(_selectedUnit, true, true);
                ClearStateAndDeselectUnitCombo();
            }
        }

        /// <summary>
        /// This function is directly binded to Controller, and links to Cancel InputAction
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public void CancelLastStateForSelectedUnit()
        {
            if (!_selectedUnit)
            {
                throw new NullReferenceException(
                    "ERROR: This Cancel function should be called ONLY when Unit Selected.");
            }

            var state = _selectedUnit.GetCurrentState();
            switch (state)
            {
                case UnitBattleState.MovingProgress:
                    Debug.LogError("State change doesn't work during Moving Progress!");
                    break;
                //case UnitBattleState.UsingAbility:
                case UnitBattleState.AbilityTargeting:
                    //if (state == UnitBattleState.UsingAbility)
                    if (_selectedUnitLastCell)
                    {
                        // TODO: Consider clean up movement buff, Consider if NEED rotation RESET (Maybe not since rotation not matter)
                        _selectedUnit.ForceMoveToCellImmediately(_selectedUnitLastCell);
                    }

                    // ONLY Place really to cancel State
                    var afterState = CancelStatePurelyForUnit(_selectedUnit, state);
                    if (afterState == UnitBattleState.Idle)
                    {
                        ClearStateAndDeselectUnit();
                    }
                    break;
                case UnitBattleState.Moving:
                    Debug.LogWarning("You are cancelling state for selected Unit!");
                    break;
                case UnitBattleState.AbilityConfirming:
                    Debug.LogWarning("State change doesn't work in AbilityConfirming!");
                    break;
                case UnitBattleState.Finished:
                case UnitBattleState.Idle:
                default:
                    break;
            }
        }

        #endregion

        private void SetupTeam(BattleTeam inTeam)
        {
            List<GridPawnUnit> units = TacticBattleManager.GetUnitsOnTeam(inTeam);
            foreach (GridPawnUnit unit in units)
            {
                unit.HandleTurnStarted();
            }
        }

        public override void Update()
        {
            switch (CurrentBattleState)
            {
                //case UnitBattleState.UsingAbility:
                case UnitBattleState.AbilityTargeting:
                    // TODO: Consider ability option
                    onUpdateSupportWithAbility.Invoke(_selectedUnit);//, CurrentAbility);
                    break;
                case UnitBattleState.Moving:
                case UnitBattleState.Finished:
                    onUpdateSupport.Invoke(_selectedUnit);
                    break;
            }
        }

        public override GridPawnUnit GetSelectedUnit()
        {
            return _selectedUnit;
        }

        public override void BeginTeamTurn(BattleTeam inTeam)
        {
            ClearStateAndDeselectUnit();
            SetupTeam(inTeam);

            if (inTeam == BattleTeam.Hostile)
            {
                bool bIsHostileTeamAI = TacticBattleManager.IsTeamAI(BattleTeam.Hostile);
                if (bIsHostileTeamAI)
                {
                    List<GridPawnUnit> aiUnits = TacticBattleManager.GetUnitsOnTeam(BattleTeam.Hostile);
                    // AStarAlgorithmUtils.RunAI(aiUnits, EndTurn);
                    // TODO: Run AI
                }
            }
        }

        /// <summary>
        /// Create Cell according to Rules, used in Unity Event, don't delete this function
        /// </summary>
        /// <param name="hit">rayCast information</param>
        /// <param name="keyIndex">Grid Cell Index</param>
        /// <param name="grid">new created grid</param>
        public void ApplyRuleOnCellCreating(RaycastHit hit, Vector2Int keyIndex, LevelGridBase grid)
        {
            int layerFlagValue = Mathf.RoundToInt(Mathf.Pow(2, hit.collider.gameObject.layer));
            if (layerFlagValue == layerMasksRuleList[0])
            {
                var cell = grid.GenerateCell(hit.point, keyIndex);
                cell.Reset();
            }
        }
    }
}