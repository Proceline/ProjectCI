using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using System;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        [NonSerialized] private LevelCellBase _selectedUnitLastCell;

        /// <summary>
        /// Asset usage in Battle Scene
        /// </summary>
        /// <param name="cell"></param>
        public void ApplyCellUnitToSelectedUnit(LevelCellBase cell)
        {
            if (!cell) return;
            // TODO: Consider Lock
            var standUnit = cell.GetUnitOnCell();

            if (!standUnit)
            {
                return;
            }

            _selectedUnitLastCell = null;

            if (standUnit is not PvMnBattleGeneralUnit playableUnit)
            {
                throw new TypeAccessException($"ONLY Type <{nameof(PvMnBattleGeneralUnit)}> can be used!");
            }

            if (standUnit.GetTeam() != CurrentTeam)
            {
                return;
            }

            if (playableUnit.IsDead() || (playableUnit.GetCurrentMovementPoints() <= 0 &&
                                          playableUnit.GetCurrentActionPoints() <= 0))
            {
                return;
            }

            if (_selectedUnit)
            {
                return;
            }

            PushStateAfterSelectUnit(playableUnit);
        }

        /// <summary>
        /// Asset usage in Battle Scene, currently Registered in Controller line 45, cell clicked
        /// Registered in AI Mono, movement determined
        /// </summary>
        /// <param name="targetCell"></param>
        public void ApplyMovementToCellForSelectedUnit(LevelCellBase targetCell)
        {
            if (!targetCell) return;
            // TODO: Handle Lock

            if (!_selectedUnit || _selectedUnit.GetCurrentState() != UnitBattleState.Moving)
            {
                return;
            }

            var standUnit = targetCell.GetUnitOnCell();
            if (standUnit && standUnit != _selectedUnit)
            {
                return;
            }

            _selectedUnitLastCell = _selectedUnit.GetCell();

            raiserTurnLockerEvent.Raise(true);
            // Move target is Self
            if (standUnit)
            {
                ChangeStateForSelectedUnit(UnitBattleState.MovingProgress);
                UpdatePlayerStateAfterRegularMove();
                raiserTurnLockerEvent.Raise(false);
            }
            else if (_selectedUnit.ExecuteMovement(targetCell, OnPathDeterminedResponse, OnVisualMovementFinished))
            {
                ChangeStateForSelectedUnit(UnitBattleState.MovingProgress);
            }
        }

        /// <summary>
        /// Asset usage in Battle Scene
        /// </summary>
        /// <param name="selectedCell"></param>
        private void ApplyAbilityToTargetCell(LevelCellBase selectedCell, CellState cellState)
        {
            if (!_selectedUnit)
            {
                return;
            }

            var state = _selectedUnit.GetCurrentState();
            if (state != UnitBattleState.AbilityTargeting) //&& state != UnitBattleState.UsingAbility)
            {
                throw new Exception("State ERROR: Must during Ability Targeting");
            }

            if (selectedCell == _selectedUnit.GetCell())
            {
                TakeRestForCurrentPlayer();
                return;
            }

            var usingAbility = cellState == CellState.ePositive ?
                _selectedUnit.SupportAbility :
                _selectedUnit.AttackAbility;

            ApplyAbilityToTargetCell(selectedCell, usingAbility);
        }

        /// <summary>
        /// Also used in AI Manager, to manually trigger ability application logic
        /// </summary>
        /// <param name="selectedCell"></param>
        /// <param name="ability"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async void ApplyAbilityToTargetCell(LevelCellBase selectedCell, PvSoUnitAbility ability)
        {
            if (!_selectedUnit)
            {
                throw new NullReferenceException("ERROR: No selected unit!");
            }

            var gridPawnUnit = selectedCell.GetUnitOnCell();
            if (!gridPawnUnit || gridPawnUnit is not PvMnBattleGeneralUnit targetUnit)
            {
                throw new NullReferenceException("ERROR: No target unit!");
            }

            raiserTurnLockerEvent.Raise(true);

            ChangeStateForSelectedUnit(UnitBattleState.AbilityConfirming);

            var queryList = CreateCombatingProcess(ability, _selectedUnit, targetUnit);
            RaiserOnCombatingListCreatedEvent.Raise(_selectedUnit, targetUnit, queryList);
            RaiserOnCombatingQueryEndEvent.Raise(_selectedUnit, targetUnit, queryList);

            foreach (var queryItem in queryList)
            {
                if (!queryItem.enabled)
                {
                    continue;
                }

                queryItem.Ability.HandleAbilityParam(queryItem.UniqueId, 
                    queryItem.holdingOwner, queryItem.targetUnit, queryItem.Commands);
            }

            RaiserCombatingTurnEndLogically.Raise(_selectedUnit, targetUnit);
            ArchiveUnitBehaviourPoints(_selectedUnit);

            // Apply visual results of commands
            foreach (var queryItem in queryList)
            {
                if (!queryItem.enabled)
                {
                    continue;
                }

                await ProcessVisualResults(queryItem);
            }

            PvAbilityQueryItem<PvMnBattleGeneralUnit>.ClearList(queryList);
            raiserTurnLockerEvent.Raise(false);
            ClearStateAndDeselectUnitCombo();
        }

        /// <summary>
        /// Mock how action will affect target unit and owner unit
        /// </summary>
        /// <param name="selectedCell"></param>
        /// <param name="ability"></param>
        public Dictionary<GridPawnUnit, int> MockAbilityToTargetCell(PvMnBattleGeneralUnit triggerUnit,
            LevelCellBase selectedCell, PvSoUnitAbility ability)
        {
            var gridPawnUnit = selectedCell.GetUnitOnCell();
            if (!gridPawnUnit || gridPawnUnit is not PvMnBattleGeneralUnit targetUnit)
            {
                throw new NullReferenceException("ERROR: No target unit!");
            }

            Dictionary<GridPawnUnit, int> allMockingDelta = new();

            var queryList = CreateCombatingProcess(ability, triggerUnit, targetUnit);
            RaiserOnCombatingListCreatedEvent.Raise(triggerUnit, targetUnit, queryList);
            RaiserOnCombatingQueryEndEvent.Raise(triggerUnit, targetUnit, queryList);

            foreach (var queryItem in queryList)
            {
                if (!queryItem.enabled)
                {
                    continue;
                }

                var queryOwner = queryItem.holdingOwner;
                var queryTarget = queryItem.targetUnit;
                var queryAbility = queryItem.Ability;

                List<LevelCellBase> effectedCells = queryAbility.GetEffectedCells(queryOwner, queryTarget.GetCell());
                foreach (AbilityParamBase param in queryAbility.GetParameters())
                {
                    foreach (var cell in effectedCells)
                    {
                        if (!queryAbility.IsAppliedOnSelf && cell == queryOwner.GetCell())
                        {
                            continue;
                        }

                        var cellUnit = cell.GetUnitOnCell();

                        if (cellUnit)
                        {
                            var result = param.MockValue(queryOwner, cellUnit, (uint)queryItem.queryOrderForm);

                            if (allMockingDelta.TryGetValue(cellUnit, out var currentDelta))
                            {
                                allMockingDelta[cellUnit] = currentDelta + result;
                            }
                            else
                            {
                                allMockingDelta[cellUnit] = result;
                            }
                        }
                    }
                }
            }


            return allMockingDelta;
        }

        private List<PvAbilityQueryItem<PvMnBattleGeneralUnit>> CreateCombatingProcess(PvSoUnitAbility ability,
            PvMnBattleGeneralUnit triggerUnit, PvMnBattleGeneralUnit targetUnit)
        {
            var queryList = PvAbilityQueryItem<PvMnBattleGeneralUnit>.CreateFirstItemList(triggerUnit, targetUnit);
            queryList[0].SetAbility(ability, ability.IsSupportAbility ? PvEnDamageForm.Support : PvEnDamageForm.Aggressive);

            if (ability.IsFollowUpAllowed())
            {
                var triggerFollowUpItem = PvAbilityQueryItem<PvMnBattleGeneralUnit>.CreateQueryItemIntoList(queryList);
                triggerFollowUpItem.holdingOwner = triggerUnit;
                triggerFollowUpItem.targetUnit = targetUnit;
                triggerFollowUpItem.SetAbility(triggerUnit.FollowUpAbility, PvEnDamageForm.Aggressive);
                triggerFollowUpItem.queryOrderForm |= PvEnDamageForm.FollowUp;
            }

            if (ability.IsCounterAllowed())
            {
                var counterAbility = targetUnit.CounterAbility;
                List<LevelCellBase> targetAbilityCells = counterAbility.GetAbilityCells(targetUnit);
                var bIsTargetAbilityAbleToCounter = targetAbilityCells.Count > 0 && targetAbilityCells.Contains(triggerUnit.GetCell());

                if (bIsTargetAbilityAbleToCounter)
                {
                    var counterItem = PvAbilityQueryItem<PvMnBattleGeneralUnit>.CreateQueryItemIntoList(queryList, 1);
                    counterItem.holdingOwner = targetUnit;
                    counterItem.targetUnit = triggerUnit;
                    counterItem.SetAbility(targetUnit.CounterAbility, PvEnDamageForm.Aggressive);
                    counterItem.queryOrderForm |= PvEnDamageForm.Counter;

                    if (counterAbility.IsFollowUpAllowed())
                    {
                        var counterFollowUpItem = PvAbilityQueryItem<PvMnBattleGeneralUnit>.CreateQueryItemIntoList(queryList);
                        counterFollowUpItem.holdingOwner = targetUnit;
                        counterFollowUpItem.targetUnit = triggerUnit;
                        counterFollowUpItem.SetAbility(targetUnit.FollowUpAbility, PvEnDamageForm.Aggressive);
                        counterFollowUpItem.queryOrderForm |= PvEnDamageForm.Counter | PvEnDamageForm.FollowUp;
                    }
                }
            }

            var abilitySpeed = triggerUnit.RuntimeAttributes.GetAttributeValue(FormulaCollectionInstance.AttackSpeedType);
            var targetSpeed = targetUnit.RuntimeAttributes.GetAttributeValue(FormulaCollectionInstance.AttackSpeedType);
            var followUpDelta = FormulaCollectionInstance.AttackSpeedDifference;

            foreach (var queryItem in queryList)
            {
                if (queryItem.holdingOwner == triggerUnit && queryItem.queryOrderForm.HasFlag(PvEnDamageForm.FollowUp))
                {
                    queryItem.enabled = abilitySpeed >= (targetSpeed + followUpDelta);
                }
                else if (queryItem.holdingOwner == targetUnit
                    && queryItem.queryOrderForm.HasFlag(PvEnDamageForm.Counter)
                    && queryItem.queryOrderForm.HasFlag(PvEnDamageForm.FollowUp))
                {
                    queryItem.enabled = targetSpeed >= (abilitySpeed + followUpDelta);
                }
            }

            return queryList;
        }

        public void AssignAbilityToCurrentUnit(PvSoUnitAbility ability)
        {
            if (!_selectedUnit)
            {
                throw new NullReferenceException("ERROR: No selected unit!");
            }

            // Empty
            // TODO: TBD
        }

        /// <summary>
        /// Take Rest,待命
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void TakeRestForCurrentPlayer()
        {
            if (!_selectedUnit)
            {
                throw new Exception("ERROR: Take Rest MUST have a Selected Unit!");
            }

            var lastUnit = _selectedUnit;
            RaiserManualFinishOrRestPrepareEvent.Raise(_selectedUnit);
            ArchiveUnitBehaviourPoints(lastUnit);
            ClearStateAndDeselectUnit();

            CheckRestUnits();
        }

        private void ArchiveUnitBehaviourPoints(PvMnBattleGeneralUnit unit, bool moveEntirelyEnd = false,
            bool actionEntirelyEnd = false)
        {
            // TODO: 再移动
            var targetActionPointResult = 0;
            var targetMovementPointResult = 0;
            unit.SetCurrentActionPoints(actionEntirelyEnd ? 0 : targetActionPointResult);
            unit.SetCurrentMovementPoints(moveEntirelyEnd ? 0 : targetMovementPointResult);
        }

        private void ClearStateAndDeselectUnitCombo()
        {
            ClearStateAndDeselectUnit();
            CheckRestUnits();
        }

        private void CheckRestUnits()
        {
            var allUnitsInBattle = _unitIdToBattleUnitHash.Values;
            var remainCount = 0;
            var team = CurrentTeam;
            foreach (var unit in allUnitsInBattle)
            {
                if (unit.GetTeam() == team &&
                    (unit.GetCurrentMovementPoints() > 0 || unit.GetCurrentActionPoints() > 0) && !unit.IsDead())
                {
                    remainCount++;
                }
            }

            if (remainCount <= 0)
            {
                Debug.Log("All Units finished actions");
                // TODO: Register Team Round End Event
                RaiserTeamRoundEndEvent.Raise(CurrentTeam);
            }
        }

#if UNITY_EDITOR
        public void EndRoundDontUseEditorOnly()
        {
            RaiserTeamRoundEndEvent.Raise(CurrentTeam);
        }
#endif
    }
}