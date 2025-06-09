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

        GridPawnUnit SelectedUnit = null;

        protected override void StartGame()
        {
            m_CurrentTeam = m_StartingTeam;
            m_TurnNumber = 0;
            m_UnitIdToBattleUnit.Clear();
            m_AbilityIdToAbility.Clear();
            
            var units = GameObject.FindObjectsByType<GridPawnUnit>(FindObjectsSortMode.None);
            foreach (var unit in units)
            {
                unit.GenerateNewID();
                if (m_UnitIdToBattleUnit.TryAdd(unit.ID, unit))
                {
                    foreach (var ability in unit.GetAbilities())
                    {
                        ability.GenerateNewID();
                        m_AbilityIdToAbility.Add(ability.ID, ability);
                    }
                }
            }

            TacticBattleManager.HandleGameStarted();

            BeginTeamTurn(m_CurrentTeam);
        }

        void CleanUpSelectedUnit()
        {
            if (SelectedUnit)
            {
                SelectedUnit.UnBindFromOnMovementPostCompleted(UpdateSelectedHoverObject);
                UnselectUnit();
            }
        }

        void SetupTeam(BattleTeam InTeam)
        {
            List<GridPawnUnit> Units = TacticBattleManager.GetUnitsOnTeam(InTeam);
            foreach (GridPawnUnit unit in Units)
            {
                unit.HandleTurnStarted();
            }
        }

        bool IsHoverObjectSpawned()
        {
            return m_CurrentHoverObject != null;
        }

        void UpdateSelectedHoverObject()
        {
            if (m_CurrentHoverObject)
            {
                Destroy(m_CurrentHoverObject);
            }

            if (SelectedUnit && !SelectedUnit.IsDead())
            {
                GameObject hoverObj = TacticBattleManager.GetSelectedHoverPrefab();
                if (hoverObj)
                {
                    m_CurrentHoverObject = Instantiate(hoverObj, SelectedUnit.GetCell().GetAllignPos(SelectedUnit), hoverObj.transform.rotation);
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if(!SelectedUnit && IsHoverObjectSpawned())
            {
                UpdateSelectedHoverObject();
            }
        }

        public void UnselectUnit()
        {
            if (SelectedUnit)
            {
                SelectedUnit.CleanUp();
                SelectedUnit = null;

                TacticBattleManager.Get().OnUnitSelected.Invoke(SelectedUnit);

                UpdateSelectedHoverObject();
            }

            TacticBattleManager.Get().UpdateHoverCells();
        }

        public override GridPawnUnit GetSelectedUnit()
        {
            return SelectedUnit;
        }

        public override void HandleNumPressed(int InNumPressed)
        {
            if(SelectedUnit)
            {
                SelectedUnit.SetupAbility(InNumPressed - 1);
            }
        }

        public override void BeginTeamTurn(BattleTeam InTeam)
        {
            CleanUpSelectedUnit();
            SetupTeam(InTeam);
            
            AilmentHandlerUtils.HandleTurnStart(InTeam);

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

        public override void EndTeamTurn(BattleTeam InTeam)
        {
            AilmentHandlerUtils.HandleTurnEnd(InTeam);
        }
        
        public override void HandleEnemySelected(GridPawnUnit InEnemyUnit)
        {
            
        }

        public override void HandlePlayerSelected(GridPawnUnit InPlayerUnit)
        {
            if (TacticBattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if (InPlayerUnit.IsDead() || InPlayerUnit.GetCurrentMovementPoints() <= 0)
            {
                return;
            }

            BattleTeam currTeam = GetCurrentTeam();
            if(currTeam == InPlayerUnit.GetTeam())
            {
                if(SelectedUnit)
                {
                    if(SelectedUnit.GetCurrentState() == UnitBattleState.UsingAbility)
                    {
                        return;
                    }

                    CleanUpSelectedUnit();
                }

                SelectedUnit = InPlayerUnit;

                if(SelectedUnit)
                {
                    SelectedUnit.SelectUnit();
                    SelectedUnit.BindToOnMovementPostCompleted(UpdateSelectedHoverObject);
                    TacticBattleManager.Get().OnUnitSelected.Invoke(SelectedUnit);
                }

                UpdateSelectedHoverObject();
            }
        }

        public override void HandleCellSelected(LevelCellBase InCell)
        {
            if (TacticBattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if(SelectedUnit)
            {
                UnitBattleState currentState = SelectedUnit.GetCurrentState();

                if (currentState == UnitBattleState.Moving)
                {
                    SelectedUnit.ExecuteMovement(InCell);
                }
                else if(currentState == UnitBattleState.UsingAbility)
                {
                    GridPawnUnit targetUnit = InCell.GetUnitOnCell();
                    if(targetUnit)
                    {
                        List<CommandResult> results = 
                            HandleAbilityCombatingLogic(SelectedUnit, targetUnit);

                        // TODO: This should be responded through Broadcast
                        HandleCommandResultsCoroutine(results);
                    }
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

        private List<CommandResult> HandleAbilityCombatingLogic(GridPawnUnit InUnit, GridPawnUnit InTargetUnit)
        {
            UnitAbilityCore ability = InUnit.GetEquippedAbility();
            UnitAbilityCore targetAbility = InTargetUnit.GetEquippedAbility();

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

            void HandleAbilityParam(UnitAbilityCore ability, GridPawnUnit InCaster, GridPawnUnit InTarget)
            {
                string resultId = Guid.NewGuid().ToString();
                foreach (AbilityParamBase param in ability.GetParameters())
                {
                    string casterId = InCaster.ID;
                    string targetId = InTarget.ID;
                    param.Execute(resultId, ability.ID, InCaster.RuntimeAttributes, casterId, 
                        InTarget.RuntimeAttributes, targetId, InTarget.GetCell(), results);
                }
            }
        }

        public override void HandleTeamWon(BattleTeam InTeam)
        {
            UnselectUnit();
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

            if (TacticBattleManager.IsPlaying() && SelectedUnit)
            {
                UnitBattleState currentState = SelectedUnit.GetCurrentState();
                if (currentState == UnitBattleState.UsingAbility)
                {
                    // TODO: Should be back to original position before moving
                    SelectedUnit.SetupMovement();
                }
                else if (currentState == UnitBattleState.Moving)
                {
                    UnselectUnit();
                }
            }
        }
    }
} 