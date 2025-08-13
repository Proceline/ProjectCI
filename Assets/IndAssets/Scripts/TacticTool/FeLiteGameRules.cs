using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;
using UnityEngine.InputSystem;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.CoreSystem.Runtime.Commands;
using System;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Abilities.Enums;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "NewGameRules", menuName = "ProjectCI Tools/GameRules/Create FeLiteGameRules", order = 1)]
    public class FeLiteGameRules : BattleGameRules
    {
        private readonly Dictionary<string, PvMnBattleGeneralUnit> _unitIdToBattleUnitHash = new();
        private readonly Dictionary<string, PvSoUnitAbility> _abilityIdToAbilityHash = new();

        private GameObject _currentHoverObject;

        [SerializeField]
        private AttributeType m_AbilitySpeedAttributeType;

        [SerializeField]
        private int m_DoubleAttackSpeedThreshold = 5;

        [NonSerialized] private PvMnBattleGeneralUnit _selectedUnit;

        [SerializeField] 
        private PvSoUnitSelectEvent selectUnitEvent;

        [SerializeField] 
        private PvSoAbilitySelectEvent selectAbilityEventByPawn;

        [SerializeField] 
        private PvSoAbilitySelectEvent confirmAbilityEvent;

        [SerializeField] 
        private PvSoTurnViewEndEvent turnViewEndEvent;

        [SerializeField] 
        private PvSoTurnLogicEndEvent turnLogicEndEvent;

        [SerializeField] 
        private LayerMask[] layerMasksRuleList;

        protected override void StartGame()
        {
            m_CurrentTeam = m_StartingTeam;
            m_TurnNumber = 0;
            _unitIdToBattleUnitHash.Clear();
            _abilityIdToAbilityHash.Clear();
            
            var units = FindObjectsByType<PvMnBattleGeneralUnit>(FindObjectsSortMode.None);
            foreach (var unit in units)
            {
                if (_unitIdToBattleUnitHash.TryAdd(unit.ID, unit))
                {
                    foreach (var ability in unit.GetUsableAbilities())
                    {
                        // TODO: Consider whether initialize ID here
                        ability.GenerateNewID();
                        _abilityIdToAbilityHash.Add(ability.ID, ability);
                    }
                }
            }

            TacticBattleManager.HandleGameStarted();

            BeginTeamTurn(m_CurrentTeam);
            
            // TODO: Add Unregister
            selectUnitEvent.RegisterCallback(SelectUnitWithParam);
            selectAbilityEventByPawn.RegisterCallback(SelectAbilityWithParam);
        }
        
        public override void HandlePlayerSelected(GridPawnUnit selectedUnit)
        {
            if (TacticBattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if (selectedUnit.IsDead() || selectedUnit.GetCurrentMovementPoints() <= 0)
            {
                return;
            }

            BattleTeam currTeam = GetCurrentTeam();
            if(currTeam == selectedUnit.GetTeam())
            {
                if(_selectedUnit)
                {
                    if(_selectedUnit.GetCurrentState() == UnitBattleState.UsingAbility)
                    {
                        return;
                    }

                    selectUnitEvent.Raise(_selectedUnit, UnitSelectBehaviour.Deselect);
                }

                if (selectedUnit is PvMnBattleGeneralUnit playableUnit)
                {
                    selectUnitEvent.Raise(playableUnit, UnitSelectBehaviour.Select);
                }

                UpdateSelectedHoverObject();
            }
        }

        private void SelectUnitWithParam(IEventOwner owner, UnitSelectEventParam selectParam)
        {
            if (selectParam.Behaviour == UnitSelectBehaviour.Select)
            {
                _selectedUnit = selectParam.Unit;
                _selectedUnit.BindToOnMovementPostCompleted(UpdatePlayerStateAfterMove);
                _selectedUnit.BindToOnMovementPostCompleted(UpdateSelectedHoverObject);
            }
            else
            {
                if (!_selectedUnit || _selectedUnit != selectParam.Unit)
                {
                    return;
                }
            
                _selectedUnit.UnBindFromOnMovementPostCompleted(UpdateSelectedHoverObject);
                _selectedUnit.UnBindFromOnMovementPostCompleted(UpdatePlayerStateAfterMove);
                
                _selectedUnit = null;
                UpdateSelectedHoverObject();
                TacticBattleManager.Get().UpdateHoverCells();
            }
        }
        
        /// <summary>
        /// This function will be triggered while selecting ability in UI
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="selectParam"></param>
        private void SelectAbilityWithParam(IEventOwner owner, AbilitySelectEventParam selectParam)
        {
            if (_selectedUnit && _selectedUnit.GetCurrentState() == UnitBattleState.UsingAbility)
            {
                var ability = selectParam.Ability;
                if (_selectedUnit.GetUsableAbilities().Contains(ability))
                {
                    // TODO: Try to decouple this part
                    _selectedUnit.SetupAbility(ability);
                    _selectedUnit.AddState(UnitBattleState.AbilityTargeting);
                    
                    // TODO: Use Event to Handle this
                    TacticBattleManager.Get().UpdateHoverCells();
                    
                    // TODO: Check and Review this
                    confirmAbilityEvent.Raise(ability);
                }
            }
        }

        void SetupTeam(BattleTeam inTeam)
        {
            List<GridPawnUnit> units = TacticBattleManager.GetUnitsOnTeam(inTeam);
            foreach (GridPawnUnit unit in units)
            {
                unit.HandleTurnStarted();
            }
        }

        bool IsHoverObjectSpawned()
        {
            return _currentHoverObject != null;
        }

        private void UpdateSelectedHoverObject()
        {
            if (_currentHoverObject)
            {
                Destroy(_currentHoverObject);
            }

            if (_selectedUnit && !_selectedUnit.IsDead())
            {
                GameObject hoverObj = TacticBattleManager.GetSelectedHoverPrefab();
                if (hoverObj)
                {
                    _currentHoverObject = Instantiate(hoverObj, _selectedUnit.GetCell().GetAllignPos(_selectedUnit), hoverObj.transform.rotation);
                }
            }
        }

        private void UpdatePlayerStateAfterMove()
        {
            if (_selectedUnit)
            {
                if (_selectedUnit.GetCurrentState() is UnitBattleState.Moving or UnitBattleState.MovingProgress)
                {
                    // TODO: Rewrite this part
                    _selectedUnit.AddState(UnitBattleState.UsingAbility);
                    // {
                    //     _selectedUnit.ClearStates();
                    // }
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if(!_selectedUnit && IsHoverObjectSpawned())
            {
                UpdateSelectedHoverObject();
            }
        }

        public override GridPawnUnit GetSelectedUnit()
        {
            return _selectedUnit;
        }

        public override void BeginTeamTurn(BattleTeam InTeam)
        {
            selectUnitEvent.Raise(_selectedUnit, UnitSelectBehaviour.Deselect);
            SetupTeam(InTeam);
            
            StatusEffectUtils.HandleTurnStart(InTeam);

            if(InTeam == BattleTeam.Hostile)
            {
                bool bIsHostileTeamAI = TacticBattleManager.IsTeamAI(BattleTeam.Hostile);
                if(bIsHostileTeamAI)
                {
                    List<GridPawnUnit> AIUnits = TacticBattleManager.GetUnitsOnTeam(BattleTeam.Hostile);
                    AStarAlgorithmUtils.RunAI(AIUnits, EndTurn);
                }
            }
        }

        public override void EndTeamTurn(BattleTeam inTeam)
        {
            StatusEffectUtils.HandleTurnEnd(inTeam);
        }
        
        public override void HandleEnemySelected(GridPawnUnit enemyUnit)
        {
            
        }

        public override List<LevelCellBase> GetAbilityHoverCells(LevelCellBase cell)
        {
            if (!_selectedUnit)
            {
                throw new NullReferenceException("ERROR: SelectedUnit or SelectedAbility is MISSING!");
            }
            
            UnitAbilityCore ability = _selectedUnit.GetCurrentUnitAbility();
            if (!ability)
            {
                throw new NullReferenceException("ERROR: Cannot identify current Ability");
            }

            List<LevelCellBase> outCells = new();
            
            if (ability)
            {
                List<LevelCellBase> abilityCells = ability.GetAbilityCells(_selectedUnit);
                List<LevelCellBase> effectedCells = ability.GetEffectedCells(_selectedUnit, cell);

                if (abilityCells.Contains(cell))
                {
                    foreach (LevelCellBase currCell in effectedCells)
                    {
                        if (currCell)
                        {
                            BattleTeam effectedTeam =
                                currCell == cell ? ability.GetEffectedTeam() : BattleTeam.All;

                            if (TacticBattleManager.CanCasterEffectTarget(_selectedUnit.GetCell(), currCell, effectedTeam,
                                    ability.DoesAllowBlocked()))
                            {
                                outCells.Add(currCell);
                            }
                        }
                    }
                }
            }

            return outCells;
        }

        public override void HandleCellSelected(LevelCellBase selectedCell)
        {
            if (TacticBattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if (_selectedUnit)
            {
                UnitBattleState currentState = _selectedUnit.GetCurrentState();

                switch (currentState)
                {
                    case UnitBattleState.Moving:
                        if (_selectedUnit.ExecuteMovement(selectedCell))
                        {
                            _selectedUnit.AddState(UnitBattleState.MovingProgress);
                        }

                        break;
                    case UnitBattleState.AbilityTargeting:
                        GridPawnUnit gridPawnUnit = selectedCell.GetUnitOnCell();
                        if (gridPawnUnit && gridPawnUnit is PvMnBattleGeneralUnit targetUnit)
                        {
                            List<CommandResult> results =
                                HandleAbilityCombatingLogic(_selectedUnit, targetUnit);

                            turnLogicEndEvent.Raise();
                            HandleCommandResultsCoroutine(results);

                            // TODO: Logically end the action, might need some event
                        }

                        break;
                }
            }
        }

        private async void HandleCommandResultsCoroutine(List<CommandResult> results)
        {
            CommandResult lastResult = null;
            LevelCellBase lastAimCell = null;
            GridPawnUnit lastOwner = null;
            List<Action<GridPawnUnit, LevelCellBase>> lastReactions = new();
            foreach (CommandResult result in results)
            {
                if (_abilityIdToAbilityHash.TryGetValue(result.AbilityId, out PvSoUnitAbility ability))
                {
                    if (lastAimCell == null)
                    {
                        RefreshAimCell(result);
                    }
                    if (lastOwner == null)
                    {
                        RefreshOwner(result);
                    }
                    if (lastResult != null && result.ResultId != lastResult.ResultId && lastOwner)
                    {
                        // Consume collected reactions and analyzed results
                        await AnalyzeResult(lastResult);

                        // Clear collected info for the next ability
                        RefreshAimCell(result);
                        RefreshOwner(result);
                        lastReactions.Clear();
                    }
                    result.AddReaction(ability, lastReactions);
                    lastResult = result;
                }
            }
            if (lastReactions.Count > 0 && lastOwner && lastAimCell)
            {
                await AnalyzeResult(lastResult);
            }
            
            _selectedUnit.AddState(UnitBattleState.Finished);
            // TODO: This should be responded through Broadcast
            selectUnitEvent.Raise(_selectedUnit, UnitSelectBehaviour.Deselect);
            
            turnViewEndEvent.Raise();

            void RefreshAimCell(CommandResult result)
            {
                LevelCellBase cell = TacticBattleManager.GetGrid()[result.TargetCellIndex];
                if (cell != null)
                {
                    lastAimCell = cell;
                }
            }

            void RefreshOwner(CommandResult result)
            {
                if (_unitIdToBattleUnitHash.TryGetValue(result.OwnerId, out PvMnBattleGeneralUnit owner))
                {
                    lastOwner = owner;
                }
            }

            async Awaitable AnalyzeResult(CommandResult result)
            {
                if (_abilityIdToAbilityHash.TryGetValue(result.AbilityId, out PvSoUnitAbility ability))
                {
                    // TODO: Handle IsAttacking if Required
                    // UnityEvent onAbilityComplete = new UnityEvent();
                    await UnitAbilityCoreExtensions.ApplyResult(ability, lastOwner, lastAimCell, lastReactions, null);
                }
            }
        }

        /// <summary>
        /// 开始进行对峙,主要攻击角色将会对目标角色发起Ability,需要取到主要角色的CurrentAbility以及可能出现目标角色的反击EquippedAbility
        /// </summary>
        /// <param name="abilityOwner">发起进攻/辅助/行动的角色</param>
        /// <param name="targetUnit">被标记的目标角色</param>
        /// <returns></returns>
        private List<CommandResult> HandleAbilityCombatingLogic(PvMnBattleGeneralUnit abilityOwner, PvMnBattleGeneralUnit targetUnit)
        {
            PvSoUnitAbility ability = abilityOwner.GetCurrentUnitAbility();
            PvSoUnitAbility targetAbility = targetUnit.GetEquippedUnitAbility();

            if (!ability || !targetAbility)
            {
                throw new NullReferenceException("ERROR: One of these two pawns missing ability!");
            }

            List<LevelCellBase> targetAbilityCells = targetAbility.GetAbilityCells(targetUnit);
            bool bIsTargetAbilityAbleToCounter = targetAbilityCells.Count > 0 && targetAbilityCells.Contains(abilityOwner.GetCell());

            int abilitySpeed = abilityOwner.RuntimeAttributes.GetAttributeValue(m_AbilitySpeedAttributeType);
            int targetAbilitySpeed = targetUnit.RuntimeAttributes.GetAttributeValue(m_AbilitySpeedAttributeType);
            FollowUpCondition followUpCondition = FollowUpCondition.None;
            if (abilitySpeed >= targetAbilitySpeed + m_DoubleAttackSpeedThreshold && ability.IsFollowUpAllowed())
            {
                followUpCondition = FollowUpCondition.InitiativeFollowUp;
            }
            else if (targetAbilitySpeed >= abilitySpeed + m_DoubleAttackSpeedThreshold && targetAbility.IsFollowUpAllowed())
            {
                followUpCondition = FollowUpCondition.CounterFollowUp;
            }

            List<CommandResult> results = new List<CommandResult>();
            List<CombatActionContext> combatActionContextList = ability.CreateCombatActionContextList(bIsTargetAbilityAbleToCounter, followUpCondition);

            foreach (CombatActionContext combatActionContext in combatActionContextList)
            {
                var combatAbility = combatActionContext.IsVictim ? targetAbility : ability;
                var caster = combatActionContext.IsVictim ? targetUnit : abilityOwner;
                var victim = combatActionContext.IsVictim ? abilityOwner : targetUnit;
                if (combatAbility)
                {
                    HandleAbilityParam(combatAbility, caster, victim);
                }
            }

            return results;

            void HandleAbilityParam(UnitAbilityCore inAbility, GridPawnUnit caster, GridPawnUnit target)
            {
                string resultId = Guid.NewGuid().ToString();
                foreach (AbilityParamBase param in ability.GetParameters())
                {
                    param.Execute(resultId, inAbility, caster, target, results);
                }
            }
        }

        public override void HandleTeamWon(BattleTeam InTeam)
        {
            selectUnitEvent.Raise(_selectedUnit, UnitSelectBehaviour.Deselect);
        }

        /// <summary>
        /// Add up extended function during cancel action applied
        /// </summary>
        /// <param name="context"></param>
        public override void CancelActionExtension(InputAction.CallbackContext context)
        {
            if (TacticBattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if (TacticBattleManager.IsPlaying() && _selectedUnit)
            {
                UnitBattleState currentState = _selectedUnit.GetCurrentState();
                // Pop current state
                _selectedUnit.RemoveLastState();
                switch (currentState)
                {
                    case UnitBattleState.UsingAbility:
                        // TODO: Should be back to original position before moving
                        _selectedUnit.SetupMovement();
                        break;
                    case UnitBattleState.Moving:
                        // Deselect Event will be raised here
                        selectUnitEvent.Raise(_selectedUnit, UnitSelectBehaviour.Deselect);
                        break;
                }
            }
        }

        /// <summary>
        /// Create Cell according to Rules, used in Unity Event, don't delete this function
        /// </summary>
        /// <param name="hit">rayCast information</param>
        /// <param name="keyIndex">Grid Cell Index</param>
        /// <param name="grid">new created grid</param>
        public void ApplyRuleOnCellCreating(RaycastHit hit, Vector2Int keyIndex, HexagonPresetGrid grid)
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