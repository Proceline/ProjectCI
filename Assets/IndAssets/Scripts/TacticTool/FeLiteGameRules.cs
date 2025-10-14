using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using System;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "NewGameRules", menuName = "ProjectCI Tools/GameRules/Create FeLiteGameRules", order = 1)]
    public partial class FeLiteGameRules : BattleGameRules
    {
        private readonly Dictionary<string, PvMnBattleGeneralUnit> _unitIdToBattleUnitHash = new();
        private readonly Dictionary<string, PvSoUnitAbility> _abilityIdToAbilityHash = new();

        private GameObject _pawnMarkObject;

        [SerializeField]
        private AttributeType abilitySpeedAttributeType;
        private const int DoubleAttackSpeedThreshold = 5;

        [NonSerialized] private PvMnBattleGeneralUnit _selectedUnit;
        [NonSerialized] private PvSoUnitAbility _selectedAbility;
        
        #region Static Action Variables

        [SerializeField] private List<PvSoUnitAbility> preloadedAbilities;
        
        #endregion

        [SerializeField] 
        private UnityEvent<PvSoUnitAbility, PvMnBattleGeneralUnit> onAbilitySelectedPostSupport;

        private PvSoUnitAbility CurrentAbility
        {
            get => _selectedAbility;
            set
            {
                _selectedAbility = value;
                if (value)
                {
                    onAbilitySelectedPostSupport?.Invoke(_selectedAbility, _selectedUnit);
                }
            }
        }

        [SerializeField] 
        private LayerMask[] layerMasksRuleList;
        
        [SerializeField] 
        private UnityEvent onGameStarted;
        
        [SerializeField] 
        private UnityEvent onGameEnded;

        [Header("Update Support")]
        [SerializeField] 
        private UnityEvent<PvMnBattleGeneralUnit> onUpdateSupport;

        [SerializeField] 
        private UnityEvent<PvMnBattleGeneralUnit, PvSoUnitAbility> onUpdateSupportWithAbility;

        [Header("Select Support")]
        
        [SerializeField] 
        private PvSoUnitSelectEvent raiserOnOwnerSelectedEvent;
        
        [SerializeField] 
        private UnityEvent<PvMnBattleGeneralUnit> onTurnOwnerSelectedPreview;
        
        [SerializeField] 
        private UnityEvent<PvMnBattleGeneralUnit> onTurnOwnerDeSelectedPreview;

        [Header("On Turn Support")]
        
        [SerializeField] 
        private PvSoTurnLogicEndEvent raiserTurnLogicallyEndEvent;
        
        [SerializeField] 
        private PvSoTurnViewEndEvent raiserTurnAnimationEndEvent;
        
        [Header("State Support")]
        
        [SerializeField]
        private PvSoUnitBattleStateEvent raiserOnStateChangedBeforeUEvent;
        
        /// <summary>
        /// Used for Controller and Models to show range and enable/disable input actions
        /// </summary>
        [SerializeField] 
        private UnityEvent<PvMnBattleGeneralUnit, UnitBattleState> onStateChangedInModel;
        public UnitBattleState CurrentBattleState =>
            _selectedUnit ? _selectedUnit.GetCurrentState() : UnitBattleState.Finished;
        
        #region Injected Fields
        
        [Inject] public static PvSoSimpleDamageApplyEvent XRaiserSimpleDamageApplyEvent;
        [Inject] private static readonly IUnitPrepareEvent RaiserManualFinishOrRestPrepareEvent;
        [Inject] private static readonly ITeamRoundEndEvent XRaiserTeamRoundEndEvent;
        [Inject] private static readonly IUnitCombatLogicFinishedEvent RaiserOnCombatLogicFinishedEvent;
        
        #endregion
        
        protected override void StartGame()
        {
            onGameStarted?.Invoke();

            CurrentTeam = BattleTeam.Friendly;
            _unitIdToBattleUnitHash.Clear();
            _abilityIdToAbilityHash.Clear();

            foreach (var staticAbility in preloadedAbilities)
            {
                if (string.IsNullOrEmpty(staticAbility.ID))
                {
                    staticAbility.GenerateNewID();
                }
                _abilityIdToAbilityHash.TryAdd(staticAbility.ID, staticAbility);
            }
            
            var units = FindObjectsByType<PvMnBattleGeneralUnit>(FindObjectsSortMode.None);
            foreach (var unit in units)
            {
                if (_unitIdToBattleUnitHash.TryAdd(unit.ID, unit))
                {
                    foreach (var ability in unit.GetUsableAbilities())
                    {
                        // TODO: Consider whether initialize ID here
                        if (string.IsNullOrEmpty(ability.ID))
                        {
                            ability.GenerateNewID();
                        }
                        _abilityIdToAbilityHash.TryAdd(ability.ID, ability);
                    }
                }
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
            onStateChangedInModel?.Invoke(selectedUnit, state);
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
            onStateChangedInModel?.Invoke(unit, stateAfterRemove);
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

            _selectedUnit = selectingUnit;
            CurrentAbility = _selectedUnit.EquippedAbility;
            _selectedUnit.BindToOnMovementPostCompleted(UpdatePlayerStateAfterRegularMove);

            ChangeStateForSelectedUnit(_selectedUnit.GetCurrentMovementPoints() > 0
                ? UnitBattleState.Moving
                : UnitBattleState.UsingAbility);
            onTurnOwnerSelectedPreview?.Invoke(selectingUnit);
            raiserOnOwnerSelectedEvent.Raise(selectingUnit, UnitSelectBehaviour.Select);
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
            raiserOnOwnerSelectedEvent.Raise(_selectedUnit, UnitSelectBehaviour.Deselect);
            _selectedUnit = null;
            CurrentAbility = null;
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

            ChangeStateForSelectedUnit(UnitBattleState.UsingAbility);
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
                case UnitBattleState.UsingAbility:
                case UnitBattleState.AbilityTargeting:
                    if (state == UnitBattleState.UsingAbility)
                    {
                        if (_selectedUnitLastCell)
                        {
                            // TODO: Consider clean up movement buff, Consider if NEED rotation RESET (Maybe not since rotation not matter)
                            _selectedUnit.ResetMovementPoints();
                            _selectedUnit.ForceMoveToCellImmediately(_selectedUnitLastCell);
                        }
                    }

                    var afterState = CancelStatePurelyForUnit(_selectedUnit, state);
                    if (afterState == UnitBattleState.Idle)
                    {
                        ClearStateAndDeselectUnit();
                    }
                    break;
                case UnitBattleState.Moving:
                    Debug.LogWarning("You are cancelling state for selected Unit!");
                    Debug.Log($"<color=yellow>Move Prepare State is directly controlled in {nameof(FeLiteGameController)}</color>");
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
                case UnitBattleState.UsingAbility:
                case UnitBattleState.AbilityTargeting:
                    // TODO: Consider ability option
                    onUpdateSupportWithAbility.Invoke(_selectedUnit, CurrentAbility);
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

            StatusEffectUtils.HandleTurnStart(inTeam);

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