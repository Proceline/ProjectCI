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

        GridPawnUnit SelectedUnit = null;

        protected override void StartGame()
        {
            m_CurrentTeam = m_StartingTeam;
            m_TurnNumber = 0;
            m_UnitIdToBattleUnit.Clear();

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
                if (m_AbilityIdToAbility.TryGetValue(result.ResultId, out UnitAbilityCore ability))
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

            int abilitySpeed = InUnit.RuntimeAttributes.GetAttributeValue(m_AbilitySpeedAttributeType);
            int targetAbilitySpeed = InTargetUnit.RuntimeAttributes.GetAttributeValue(m_AbilitySpeedAttributeType);
            
            List<CommandResult> results = new List<CommandResult>();
            if (ability)
            {
                m_AbilityIdToAbility.TryAdd(ability.GetAbilityId(InUnit.ID), ability);
                HandleAbilityParam(ability, InUnit, InTargetUnit);
            }

            if (targetAbility && ability.IsAbilityCounterAllowed())
            {
                m_AbilityIdToAbility.TryAdd(targetAbility.GetAbilityId(InTargetUnit.ID), targetAbility);
                HandleAbilityParam(targetAbility, InTargetUnit, InUnit);
            }

            if (abilitySpeed >= targetAbilitySpeed)
            {
                HandleAbilityParam(ability, InUnit, InTargetUnit);
            }
            else if (targetAbilitySpeed > abilitySpeed)
            {
                HandleAbilityParam(targetAbility, InTargetUnit, InUnit);
            }

            return results;

            void HandleAbilityParam(UnitAbilityCore ability, GridPawnUnit InCaster, GridPawnUnit InTarget)
            {
                foreach (AbilityParamBase param in ability.GetParameters())
                {
                    string casterId = InCaster.ID;
                    string targetId = InTarget.ID;
                    // TODO: Unit assignment to Dictionary should be done in the BattleManager
                    m_UnitIdToBattleUnit.TryAdd(casterId, InCaster);
                    m_UnitIdToBattleUnit.TryAdd(targetId, InTarget);
                    param.Execute(ability.GetAbilityId(casterId), InCaster.RuntimeAttributes, casterId, 
                        InTarget.RuntimeAttributes, targetId, InTarget.GetCell(), results);
                }
            }
        }

        public override void HandleTeamWon(BattleTeam InTeam)
        {
            UnselectUnit();
        }

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