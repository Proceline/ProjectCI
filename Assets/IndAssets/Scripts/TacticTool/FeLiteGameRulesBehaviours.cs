using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.Commands;
using System;
using ProjectCI.CoreSystem.Runtime.Abilities;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        [Header("View Strong Links")]
        [SerializeField] private UnityEvent<PvSoUnitAbility, PvMnBattleGeneralUnit> onPreviewAbilitiesIndex;
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onResetDefaultAbilitiesRange;
        
        /// <summary>
        /// Asset usage in Battle Scene
        /// </summary>
        /// <param name="cell"></param>
        public void ApplyCellUnitToSelectedUnit(LevelCellBase cell)
        {
            if (!cell) return;
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
            if (!targetCell) return;
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

                raiserTurnLogicallyEndEvent.Raise();
                HandleCommandResultsCoroutine(results);

                // TODO: Logically end the action, might need some event
            }
        }

        /// <summary>
        /// Normally used in Buttons with ButtonIndex
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void PreviewPlayerAbilityInModel(int index = -1)
        {
            if (!_selectedUnit)
            {
                return;
            }

            if (index == -1)
            {
                onPreviewAbilitiesIndex.Invoke(_selectedUnit.EquippedAbility, _selectedUnit);
                return;
            }

            var attacks = _selectedUnit.GetAttackAbilities();
            if (index >= attacks.Count)
            {
                throw new IndexOutOfRangeException();
            }

            onPreviewAbilitiesIndex.Invoke(attacks[index], _selectedUnit);
        }

        /// <summary>
        /// Normally used in Buttons with ButtonIndex
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void PreviewPlayerSupportInModel(int index = -1)
        {

            if (!_selectedUnit)
            {
                return;
            }

            if (index == -1)
            {
                var defaultSupport = _selectedUnit.DefaultSupport;
                if (defaultSupport)
                {
                    onPreviewAbilitiesIndex.Invoke(defaultSupport, _selectedUnit);
                }

                return;
            }

            var supports = _selectedUnit.GetSupportAbilities();
            if (index >= supports.Count)
            {
                throw new IndexOutOfRangeException();
            }

            onPreviewAbilitiesIndex.Invoke(supports[index], _selectedUnit);
        }

        public void ResetPlayerDefaultAbilities()
        {
            if (!_selectedUnit)
            {
                throw new NullReferenceException();
            }
            
            onResetDefaultAbilitiesRange.Invoke(_selectedUnit);
        }
    }
}