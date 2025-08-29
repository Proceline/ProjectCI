using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.CoreSystem.Runtime.Commands;
using System;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Abilities.Enums;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        /// <summary>
        /// Asset usage in Battle Scene
        /// </summary>
        /// <param name="cell"></param>
        public void ApplyCellUnitToSelectedUnit(LevelCellBase cell)
        {
            // TODO: Consider Lock
            GridPawnUnit standUnit = cell.GetUnitOnCell();
            if (standUnit is PvMnBattleGeneralUnit playableUnit)
            {
                if (standUnit.GetTeam() == CurrentTeam)
                {
                    if (playableUnit.IsDead() || playableUnit.GetCurrentMovementPoints() <= 0)
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
            // TODO: Handle Lock

            if (!_selectedUnit || _selectedUnit.GetCurrentState() != UnitBattleState.Moving)
            {
                return;
            }
            if (_selectedUnit.ExecuteMovement(targetCell))
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
            // TODO: Handle Lock

            if (!_selectedUnit)
            {
                return;
            }

            var state = _selectedUnit.GetCurrentState();
            if (state != UnitBattleState.AbilityTargeting && state != UnitBattleState.UsingAbility)
            {
                throw new Exception("State ERROR: Must during Ability Targeting");
            }

            GridPawnUnit gridPawnUnit = selectedCell.GetUnitOnCell();
            if (gridPawnUnit && gridPawnUnit is PvMnBattleGeneralUnit targetUnit)
            {
                List<CommandResult> results =
                    HandleAbilityCombatingLogic(_selectedUnit, targetUnit);

                turnLogicEndEvent.Raise();
                HandleCommandResultsCoroutine(results);

                // TODO: Logically end the action, might need some event
            }
        }
    }
}