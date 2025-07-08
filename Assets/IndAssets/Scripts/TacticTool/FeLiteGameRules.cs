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
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Abilities.Enums;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "NewGameRules", menuName = "ProjectCI Tools/GameRules/Create FeLiteGameRules", order = 1)]
    public class FeLiteGameRules : BattleGameRules
    {
        private readonly Dictionary<string, GridPawnUnit> m_UnitIdToBattleUnit = new();
        private readonly Dictionary<string, UnitAbilityCore> m_AbilityIdToAbility = new();

        GameObject m_CurrentHoverObject;

        [SerializeField]
        private AttributeType m_AbilitySpeedAttributeType;

        [SerializeField]
        private int m_DoubleAttackSpeedThreshold = 5;

        [NonSerialized] private GridPawnUnit _selectedUnit;
        [NonSerialized] private UnitAbilityCore _selectedAbility;

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
            m_UnitIdToBattleUnit.Clear();
            m_AbilityIdToAbility.Clear();
            
            var units = GameObject.FindObjectsByType<GridPawnUnit>(FindObjectsSortMode.None);
            foreach (var unit in units)
            {
                if (m_UnitIdToBattleUnit.TryAdd(unit.ID, unit))
                {
                    foreach (var ability in unit.GetAbilities())
                    {
                        // TODO: Consider whether initialize ID here
                        ability.GenerateNewID();
                        m_AbilityIdToAbility.Add(ability.ID, ability);
                    }
                }
            }

            TacticBattleManager.HandleGameStarted();

            BeginTeamTurn(m_CurrentTeam);
            
            // TODO: Add Unregister
            selectUnitEvent.RegisterCallback(SelectUnitWithParam);
            selectAbilityEventByPawn.RegisterCallback(SelectAbilityWithParam);
        }
        
        public override void HandlePlayerSelected(GridPawnUnit inPlayerUnit)
        {
            if (TacticBattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if (inPlayerUnit.IsDead() || inPlayerUnit.GetCurrentMovementPoints() <= 0)
            {
                return;
            }

            BattleTeam currTeam = GetCurrentTeam();
            if(currTeam == inPlayerUnit.GetTeam())
            {
                if(_selectedUnit)
                {
                    if(_selectedUnit.GetCurrentState() == UnitBattleState.UsingAbility)
                    {
                        return;
                    }

                    selectUnitEvent.Raise(_selectedUnit, UnitSelectBehaviour.Deselect);
                }

                if (inPlayerUnit)
                {
                    selectUnitEvent.Raise(inPlayerUnit, UnitSelectBehaviour.Select);
                }

                UpdateSelectedHoverObject();
            }
        }

        private void SelectUnitWithParam(IEventOwner owner, UnitSelectEventParam selectParam)
        {
            if (selectParam.Behaviour == UnitSelectBehaviour.Select)
            {
                _selectedUnit = selectParam.GridPawnUnit;
                _selectedUnit.BindToOnMovementPostCompleted(UpdatePlayerStateAfterMove);
                _selectedUnit.BindToOnMovementPostCompleted(UpdateSelectedHoverObject);
                _selectedAbility = null;
            }
            else
            {
                if (!_selectedUnit || _selectedUnit != selectParam.GridPawnUnit)
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
        
        private void SelectAbilityWithParam(IEventOwner owner, AbilitySelectEventParam selectParam)
        {
            if (_selectedUnit && _selectedUnit.GetCurrentState() == UnitBattleState.UsingAbility)
            {
                var ability = selectParam.Ability;
                if (_selectedUnit.GetAbilities().Contains(ability))
                {
                    _selectedAbility = ability;
                    _selectedUnit.SetupAbility(ability);
                    _selectedUnit.AddState(UnitBattleState.AbilityTargeting);
                    
                    // TODO: Use Event to Handle this
                    TacticBattleManager.Get().UpdateHoverCells();
                }
                _selectedAbility = selectParam.Ability;
                confirmAbilityEvent.Raise(ability);
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
            return m_CurrentHoverObject != null;
        }

        private void UpdateSelectedHoverObject()
        {
            if (m_CurrentHoverObject)
            {
                Destroy(m_CurrentHoverObject);
            }

            if (_selectedUnit && !_selectedUnit.IsDead())
            {
                GameObject hoverObj = TacticBattleManager.GetSelectedHoverPrefab();
                if (hoverObj)
                {
                    m_CurrentHoverObject = Instantiate(hoverObj, _selectedUnit.GetCell().GetAllignPos(_selectedUnit), hoverObj.transform.rotation);
                }
            }
        }

        private void UpdatePlayerStateAfterMove()
        {
            if (_selectedUnit != null)
            {
                if (_selectedUnit.GetCurrentState() is UnitBattleState.Moving or UnitBattleState.MovingProgress)
                {
                    if (_selectedUnit.GetCurrentAbilityPoints() > 0)
                    {
                        _selectedUnit.AddState(UnitBattleState.UsingAbility);
                    }
                    else
                    {
                        _selectedUnit.ClearStates();
                    }
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
        
        public override void HandleEnemySelected(GridPawnUnit InEnemyUnit)
        {
            
        }

        public override List<LevelCellBase> GetAbilityHoverCells(LevelCellBase InCell)
        {
            if (!_selectedUnit)
            {
                throw new NullReferenceException("ERROR: SelectedUnit or SelectedAbility is MISSING!");
            }
            
            UnitAbilityCore ability = _selectedAbility;
            if (!_selectedAbility)
            {
                Debug.LogError("ERROR: Cannot identify current Ability");
                ability = _selectedUnit.GetCurrentAbility();
            }

            List<LevelCellBase> outCells = new();
            
            if (ability)
            {
                List<LevelCellBase> abilityCells = ability.GetAbilityCells(_selectedUnit);
                List<LevelCellBase> effectedCells = ability.GetEffectedCells(_selectedUnit, InCell);

                if (abilityCells.Contains(InCell))
                {
                    foreach (LevelCellBase currCell in effectedCells)
                    {
                        if (currCell)
                        {
                            BattleTeam effectedTeam =
                                (currCell == InCell) ? ability.GetEffectedTeam() : BattleTeam.All;

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

        public override void HandleCellSelected(LevelCellBase InCell)
        {
            if (TacticBattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if(_selectedUnit)
            {
                UnitBattleState currentState = _selectedUnit.GetCurrentState();

                switch (currentState)
                {
                    case UnitBattleState.Moving:
                        if (_selectedUnit.ExecuteMovement(InCell))
                        {
                            _selectedUnit.AddState(UnitBattleState.MovingProgress);
                        }
                        break;
                    case UnitBattleState.AbilityTargeting:
                        GridPawnUnit targetUnit = InCell.GetUnitOnCell();
                        if(targetUnit)
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
                if (m_AbilityIdToAbility.TryGetValue(result.AbilityId, out UnitAbilityCore ability))
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
                if (m_UnitIdToBattleUnit.TryGetValue(result.OwnerId, out GridPawnUnit owner))
                {
                    lastOwner = owner;
                }
            }

            async Awaitable AnalyzeResult(CommandResult result)
            {
                if (m_AbilityIdToAbility.TryGetValue(result.AbilityId, out UnitAbilityCore ability))
                {
                    await lastOwner.ShowResult(ability, lastAimCell, lastReactions);
                }
            }
        }

        /// <summary>
        /// Used to apply abilities and end ability
        /// </summary>
        /// <param name="InUnit"></param>
        /// <param name="InTargetUnit"></param>
        /// <returns></returns>
        private List<CommandResult> HandleAbilityCombatingLogic(GridPawnUnit InUnit, GridPawnUnit InTargetUnit)
        {
            UnitAbilityCore ability = _selectedAbility? _selectedAbility : InUnit.GetCurrentAbility();
            UnitAbilityCore targetAbility = InTargetUnit.GetCurrentAbility();

            List<LevelCellBase> targetAbilityCells = targetAbility.GetAbilityCells(InTargetUnit);
            bool bIsTargetAbilityAbleToCounter = targetAbilityCells.Count > 0 && targetAbilityCells.Contains(InUnit.GetCell());

            int abilitySpeed = InUnit.RuntimeAttributes.GetAttributeValue(m_AbilitySpeedAttributeType);
            int targetAbilitySpeed = InTargetUnit.RuntimeAttributes.GetAttributeValue(m_AbilitySpeedAttributeType);
            FollowUpCondition followUpCondition = FollowUpCondition.None;
            if (abilitySpeed >= targetAbilitySpeed + m_DoubleAttackSpeedThreshold && ability.IsAbilityFollowUpAllowed())
            {
                followUpCondition = FollowUpCondition.InitiativeFollowUp;
            }
            else if (targetAbilitySpeed >= abilitySpeed + m_DoubleAttackSpeedThreshold && targetAbility.IsAbilityFollowUpAllowed())
            {
                followUpCondition = FollowUpCondition.CounterFollowUp;
            }

            List<CommandResult> results = new List<CommandResult>();
            List<CombatActionContext> combatActionContextList = ability.CreateCombatActionContextList(bIsTargetAbilityAbleToCounter, followUpCondition);

            foreach (CombatActionContext combatActionContext in combatActionContextList)
            {
                var combatAbility = combatActionContext.IsVictim ? targetAbility : ability;
                var caster = combatActionContext.IsVictim ? InTargetUnit : InUnit;
                var victim = combatActionContext.IsVictim ? InUnit : InTargetUnit;
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