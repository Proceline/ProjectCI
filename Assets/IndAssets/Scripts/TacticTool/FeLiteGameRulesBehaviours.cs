using IndAssets.Scripts.Abilities;
using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Deployment;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.Utilities.Runtime.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        [SerializeField] private PvSoBattleState gameBattleState;
        [NonSerialized] private LevelCellBase _selectedUnitLastCell;

        private readonly HashSet<PvMnBattleGeneralUnit> _finishedPlayableUnits = new();
        private readonly HashSet<PvMnBattleGeneralUnit> _ultPreparedUnits = new();

        private void ApplyCellUnitToSelectedUnit(LevelCellBase cell)
        {
            if (gameBattleState.GetCurrentState != PvPlayerRoundState.None)
            {
                return;
            }

            if (!cell)
            {
                return;
            }

            var standUnit = cell.GetUnitOnCell();
            if (!standUnit || _selectedUnit || standUnit.GetTeam() != CurrentTeam || standUnit.IsDead())
            {
                return;
            }

            if (standUnit is not PvMnBattleGeneralUnit playableUnit)
            {
                throw new TypeAccessException($"ONLY Type <{nameof(PvMnBattleGeneralUnit)}> can be used!");
            }

            if (_finishedPlayableUnits.Contains(playableUnit))
            {
                return;
            }

            _selectedUnitLastCell = null;

            SelectUnit(playableUnit);
            gameBattleState.PushState(playableUnit.IsInUltimateForm ? 
                PvPlayerRoundState.Prepare : PvPlayerRoundState.Selected, playableUnit);
        }

        private void ApplyMovementToCellForSelectedUnit(LevelCellBase targetCell)
        {
            if (!targetCell) return;

            var playingUnit = _selectedUnit;

            var standUnit = targetCell.GetUnitOnCell();
            if (standUnit && standUnit != playingUnit)
            {
                return;
            }

            _selectedUnitLastCell = playingUnit.GetCell();

            gameBattleState.PushState(PvPlayerRoundState.Moving, playingUnit);

            ApplyMovementToCell(playingUnit, targetCell,
                () => 
                {
                    if (gameBattleState.GetCurrentState == PvPlayerRoundState.Moving)
                    {
                        gameBattleState.PopLastState();
                        gameBattleState.PushState(PvPlayerRoundState.Prepare, playingUnit);
                    } 
                });
        }

        private void ApplyAbilityToTargetCell(LevelCellBase selectedCell, CellState cellState)
        {
            var playingUnit = _selectedUnit;

            if (selectedCell == playingUnit.GetCell())
            {
                TakeRestForCurrentPlayer();
                return;
            }

            var usingAbility = cellState == CellState.ePositive ? playingUnit.SupportAbility : playingUnit.AttackAbility;
            if (playingUnit.IsInUltimateForm)
            {
                usingAbility = playingUnit.UltimateAbility;
            }

            playingUnit.StartCoroutine(StartAbilityApplyCoroutine(playingUnit, selectedCell, usingAbility));
        }

        private IEnumerator StartAbilityApplyCoroutine(PvMnBattleGeneralUnit triggerUnit, LevelCellBase selectedCell, PvSoUnitAbility ability)
        {
            gameBattleState.PushState(PvPlayerRoundState.Applying, triggerUnit);
            yield return ApplyAbility(triggerUnit, selectedCell, ability);
            FinishUnitAction(triggerUnit, false);

            _ = CheckStageClearStatus();
        }

        public bool CheckStageClearStatus()
        {
            if (PvSoLevelData.LoadingLevel &&
                PvSoLevelData.LoadingLevel.CheckIfLevelCompleted(_unitIdToBattleUnitHash) == PvTargetCompleteCondition.Completed)
            {
                raiserGamePreEndedEvent?.Raise();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Take Rest
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void TakeRestForCurrentPlayer()
        {
            var lastUnit = _selectedUnit;
            RaiserManualFinishOrRestPrepareEvent.Raise(lastUnit);
            FinishUnitAction(lastUnit, true);
        }

        private void FinishUnitAction(PvMnBattleGeneralUnit finishedUnit, bool ultimate)
        {
            gameBattleState.PushState(PvPlayerRoundState.None, null);

            if (finishedUnit)
            {
                DeselectUnit(finishedUnit);
                if (ultimate)
                {
                    _ultPreparedUnits.Add(finishedUnit);
                }
                else
                {
                    _finishedPlayableUnits.Add(finishedUnit);
                }
            }

            CheckRestUnits();
        }

        private void SelectUnit(PvMnBattleGeneralUnit selectingUnit)
        {
            if (_ultPreparedUnits.Contains(selectingUnit))
            {
                selectingUnit.SwitchForm(true);
            }

            _selectedUnit = selectingUnit;
            onTurnOwnerSelectedPreview?.Invoke(selectingUnit);
            raiserOwnerSelectedViewEvent.Raise(selectingUnit, UnitSelectBehaviour.Select);
        }

        private void DeselectUnit(PvMnBattleGeneralUnit selectingUnit)
        {
            if (_ultPreparedUnits.Contains(selectingUnit))
            {
                selectingUnit.SwitchForm(false);
            }

            onTurnOwnerDeSelectedPreview?.Invoke(selectingUnit);
            raiserOwnerSelectedViewEvent.Raise(selectingUnit, UnitSelectBehaviour.Deselect);
            _selectedUnit = null;
        }

        /// <summary>
        /// This function is manually binded to State Machine Object
        /// links to Cancel InputAction
        /// </summary>
        public void RevertActionResponse(PvPlayerRoundState stateToBeCancelled)
        {
            var playingUnit = _selectedUnit;
            switch (stateToBeCancelled)
            {
                case PvPlayerRoundState.Prepare:
                    if (playingUnit.IsInUltimateForm)
                    {
                        DeselectUnit(playingUnit);
                    }
                    else if (_selectedUnitLastCell && playingUnit.GetCell() != _selectedUnitLastCell)
                    {
                        // TODO: Clean up movement buff
                        playingUnit.ForceMoveToCellImmediately(_selectedUnitLastCell);
                    }
                    break;
                case PvPlayerRoundState.Selected:
                    DeselectUnit(playingUnit);
                    break;
                default:
                    break;
            }
        }

        public void ApplyMovementToCell(PvMnBattleGeneralUnit triggerUnit, LevelCellBase targetCell)
        {
            var standUnit = targetCell.GetUnitOnCell();
            if (standUnit && standUnit != triggerUnit)
            {
                throw new Exception($"ERROR: {triggerUnit.name} Try to move on cell with Pawn<{standUnit.name}>");
            }

            if (!standUnit)
            {
                raiserTurnLockerEvent.Raise(true);
                var movable = triggerUnit.ExecuteMovement(targetCell, 
                    path => onPathDeterminedSupport?.Invoke(triggerUnit, path), 
                    () => raiserTurnLockerEvent.Raise(false));

                if (!movable)
                {
                    raiserTurnLockerEvent.Raise(false);
                }
            }
        }

        private void ApplyMovementToCell(PvMnBattleGeneralUnit triggerUnit, LevelCellBase targetCell, Action onMovementCompleted)
        {
            var standUnit = targetCell.GetUnitOnCell();
            if (standUnit && standUnit != triggerUnit)
            {
                throw new Exception($"ERROR: {triggerUnit.name} Try to move on cell with Pawn<{standUnit.name}>");
            }

            if (!standUnit)
            {
                raiserTurnLockerEvent.Raise(true);
                var movable = triggerUnit.ExecuteMovement(targetCell,
                    path => onPathDeterminedSupport?.Invoke(triggerUnit, path),
                    () => 
                    {
                        onMovementCompleted.Invoke();
                        raiserTurnLockerEvent.Raise(false);
                    });

                if (!movable)
                {
                    onMovementCompleted.Invoke();
                    raiserTurnLockerEvent.Raise(false);
                }
            }
            else
            {
                onMovementCompleted.Invoke();
            }
        }

        /// <summary>
        /// Also used in AI Manager, to manually trigger ability application logic
        /// </summary>
        /// <param name="selectedCell"></param>
        /// <param name="ability"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async Awaitable ApplyAbility(PvMnBattleGeneralUnit triggerUnit, LevelCellBase selectedCell, PvSoUnitAbility ability)
        {
            var gridPawnUnit = selectedCell.GetUnitOnCell();
            if (!gridPawnUnit || gridPawnUnit is not PvMnBattleGeneralUnit targetUnit)
            {
                throw new NullReferenceException("ERROR: No target unit!");
            }

            raiserTurnLockerEvent.Raise(true);

            var queryList = CreateCombatingProcess(ability, triggerUnit, targetUnit);
            RaiserOnCombatingListCreatedEvent.Raise(triggerUnit, targetUnit, queryList);
            RaiserOnCombatingQueryEndEvent.Raise(triggerUnit, targetUnit, queryList);

            foreach (var queryItem in queryList)
            {
                if (!queryItem.enabled)
                {
                    continue;
                }

                queryItem.Ability.HandleAbilityParam(queryItem.UniqueId, 
                    queryItem.holdingOwner, queryItem.targetUnit, queryItem.Commands);
            }

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
            MockAbilityToTargetCell(ref allMockingDelta, triggerUnit, targetUnit, ability);
            return allMockingDelta;
        }

        private void MockAbilityToTargetCell(ref Dictionary<GridPawnUnit, int> results, PvMnBattleGeneralUnit triggerUnit,
            PvMnBattleGeneralUnit targetUnit, PvSoUnitAbility ability)
        {
            results.Clear();

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

                        if (!cellUnit)
                        {
                            continue;
                        }

                        var result = param.MockValue(queryOwner, cellUnit, (uint)queryItem.queryOrderForm);

                        if (results.TryGetValue(cellUnit, out var currentDelta))
                        {
                            results[cellUnit] = currentDelta + result;
                        }
                        else
                        {
                            results[cellUnit] = result;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Binded to PvMnGameVisualBridges
        /// </summary>
        /// <param name="results"></param>
        /// <param name="targetCell"></param>
        public void GetMockingDataForSelectedUnit(Dictionary<GridPawnUnit, int> results, LevelCellBase targetCell)
        {
            var selectedUnit = _selectedUnit;

            if (selectedUnit)
            {
                var gridPawnUnit = targetCell.GetUnitOnCell();
                if (!gridPawnUnit || gridPawnUnit is not PvMnBattleGeneralUnit targetUnit)
                {
                    throw new NullReferenceException("ERROR: No target unit!");
                }

                var usingAbility = TacticBattleManager.GetTeamAffinity(selectedUnit.GetTeam(), targetUnit.GetTeam()) == BattleTeam.Friendly ?
                    selectedUnit.SupportAbility : selectedUnit.AttackAbility;
                if (selectedUnit.IsInUltimateForm)
                {
                    usingAbility = selectedUnit.UltimateAbility;
                }

                MockAbilityToTargetCell(ref results, selectedUnit, targetUnit, usingAbility);
            }
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

        private void CheckRestUnits()
        {
            var allUnitsInBattle = _unitIdToBattleUnitHash.Values;
            var requiredCount = 0;

            foreach (var unit in allUnitsInBattle)
            {
                if (unit.GetTeam() == CurrentTeam && !unit.IsDead())
                {
                    requiredCount++;
                }
            }

            if (_finishedPlayableUnits.Count >= requiredCount)
            {
                EndRound();
            }
        }

        public async void EndRound()
        {
            if (_teamRoundEndDelayList.Count > 0)
            {
                _teamRoundEndDelayList.Clear();
            }

            _finishedPlayableUnits.Clear();

            foreach (var ultUnit in _ultPreparedUnits)
            {
                ultUnit.SwitchForm(false);
            }
            _ultPreparedUnits.Clear();

            var index = 0;
            foreach (var roundEndUnityEvent in roundEventEndList)
            {
                roundEndUnityEvent.Invoke(CurrentTeam, _teamRoundEndDelayList);
                if (index < _teamRoundEndDelayList.Count)
                {
                    var waitTime = _teamRoundEndDelayList[index];
                    if (waitTime > 0.01f)
                    {
                        await Awaitable.WaitForSecondsAsync(waitTime);
                    }
                    index++;
                }
            }

            RaiserTeamRoundEndEvent.Raise(CurrentTeam);
            _teamRoundEndDelayList.Clear();
        }

    }
}