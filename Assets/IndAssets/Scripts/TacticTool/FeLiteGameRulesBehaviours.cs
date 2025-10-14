using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using UnityEngine;

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
            _selectedUnitLastCell = null;
            
            if (standUnit is PvMnBattleGeneralUnit playableUnit)
            {
                if (standUnit.GetTeam() == CurrentTeam)
                {
                    if (playableUnit.IsDead())// || playableUnit.GetCurrentMovementPoints() <= 0)
                    {
                        return;
                    }

                    if (_selectedUnit)
                    {
                        return;
                    }

                    PushStateAfterSelectUnit(playableUnit);
                }
            }
        }

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
                // TODO: Consider move to Unit = Ride/Be Ride
                var teamSituation =
                    TacticBattleManager.GetTeamAffinity(targetCell.GetCellTeam(), _selectedUnit.GetTeam());
                Debug.Log(
                    $"CELL<{targetCell.GetIndex().ToString()}> already be occupied by Pawn, Team is <color=green>{teamSituation.ToString()}</color>");
                return;
            }

            _selectedUnitLastCell = _selectedUnit.GetCell();
            if (standUnit)
            {
                ChangeStateForSelectedUnit(UnitBattleState.MovingProgress);
                UpdatePlayerStateAfterRegularMove();
            }
            else if (_selectedUnit.ExecuteMovement(targetCell))
            {
                ChangeStateForSelectedUnit(UnitBattleState.MovingProgress);
            }
        }

        /// <summary>
        /// Asset usage in Battle Scene
        /// </summary>
        /// <param name="selectedCell"></param>
        public void ApplyAbilityToTargetCell(LevelCellBase selectedCell)
        {
            if (!_selectedUnit)
            {
                return;
            }

            var effectTeam = CurrentAbility.GetEffectedTeam();
            var cellState = selectedCell.GetCellState();
            if ((effectTeam == BattleTeam.Hostile && cellState != CellState.eNegative) ||
                (effectTeam == BattleTeam.Friendly && cellState != CellState.ePositive))
            {
                return;
            }

            var state = _selectedUnit.GetCurrentState();
            if (state != UnitBattleState.AbilityTargeting && state != UnitBattleState.UsingAbility)
            {
                throw new Exception("State ERROR: Must during Ability Targeting");
            }

            ChangeStateForSelectedUnit(UnitBattleState.AbilityConfirming);

            var gridPawnUnit = selectedCell.GetUnitOnCell();
            if (!gridPawnUnit || gridPawnUnit is not PvMnBattleGeneralUnit targetUnit)
            {
                return;
            }

            var results = new Queue<CommandResult>();
            RaiserOnCombatLogicPreEvent.Raise(_selectedUnit, CurrentAbility, targetUnit, results);
            {
                HandleAbilityCombatingLogic(_selectedUnit, targetUnit, ref results);
            }
            RaiserOnCombatLogicPostEvent.Raise(_selectedUnit, CurrentAbility, targetUnit, results);
            raiserTurnLogicallyEndEvent.Raise();
            HandleCommandResultsCoroutine(results);
        }

        public void AssignAbilityToCurrentUnit(PvSoUnitAbility ability)
        {
            if (!_selectedUnit)
            {
                throw new NullReferenceException("ERROR: No selected unit!");
            }
            CurrentAbility = ability;
        }

        public void TakeRestForCurrentPlayer()
        {
            if (!_selectedUnit)
            {
                throw new Exception("ERROR: Take Rest MUST have a Selected Unit!");
            }

            var lastUnit = _selectedUnit;
            RaiserManualFinishOrRestPrepareEvent.Raise(_selectedUnit);
            ClearStateAndDeselectUnit();

            var team = lastUnit.GetTeam();
            var allUnitsInBattle = _unitIdToBattleUnitHash.Values;
            var remainCount = 0;
            foreach (var unit in allUnitsInBattle)
            {
                if (unit.GetTeam() == team && unit.GetCurrentMovementPoints() > 0 && !unit.IsDead())
                {
                    remainCount++;
                }
            }

            if (remainCount <= 0)
            {
                Debug.Log("All Units finished actions");
                // TODO: Register Team Round End Event
                XRaiserTeamRoundEndEvent.Raise(CurrentTeam);
            }
        }
    }
}