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
            
            // Move target is Self
            if (standUnit)
            {
                ChangeStateForSelectedUnit(UnitBattleState.MovingProgress);
                UpdatePlayerStateAfterRegularMove();
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

            var commandResults = new Queue<CommandResult>();
            RaiserOnCombatLogicPreEvent.Raise(_selectedUnit, CurrentAbility, targetUnit, commandResults);
            {
                HandleAbilityCombatingLogic(_selectedUnit, targetUnit, ref commandResults);
            }
            RaiserOnCombatLogicPostEvent.Raise(_selectedUnit, CurrentAbility, targetUnit, commandResults);
            RaiserCombatingTurnEndLogically.Raise(_selectedUnit, targetUnit);
            ArchiveUnitBehaviourPoints(_selectedUnit);
            
            // ClearStateAndDeselectUnitCombo func applied in HandleCommandResultsCoroutine
            HandleCommandResultsCoroutine(commandResults);
        }

        public void AssignAbilityToCurrentUnit(PvSoUnitAbility ability)
        {
            if (!_selectedUnit)
            {
                throw new NullReferenceException("ERROR: No selected unit!");
            }
            CurrentAbility = ability;
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
            unit.SetCurrentActionPoints(actionEntirelyEnd? 0 : targetActionPointResult);
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
                XRaiserTeamRoundEndEvent.Raise(CurrentTeam);
            }
        }

        private void OnTeamRoundEndResponse(BattleTeam team)
        {
            var allUnitsInBattle = _unitIdToBattleUnitHash.Values;
            foreach (var unit in allUnitsInBattle)
            {
                if (unit.GetTeam() == team && !unit.IsDead())
                {
                    ArchiveUnitBehaviourPoints(unit, true, true);
                }
            }

            CurrentTeam = team == BattleTeam.Friendly? BattleTeam.Hostile : BattleTeam.Friendly;
            BeginTeamTurn(CurrentTeam);
        }
        
        #if UNITY_EDITOR
        public void EndRoundDontUseEditorOnly()
        {
            XRaiserTeamRoundEndEvent.Raise(CurrentTeam);
        }
        #endif
    }
}