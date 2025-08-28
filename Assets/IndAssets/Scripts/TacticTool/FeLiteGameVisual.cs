using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    
    [CreateAssetMenu(fileName = "New Visual", menuName = "ProjectCI Tools/MVC/Visual", order = 1)]
    public class FeLiteGameVisual : ScriptableObject, IGameVisual
    {
        internal LevelCellBase CurrentHoverCell { get; private set; }
        private readonly List<LevelCellBase> _bufferedVisualStateCells = new();
        private readonly List<LevelCellBase> _hoveringCells = new();
        
        void IGameVisual.OnVisualUpdate()
        {
            UpdateHoverCells();
        }
        
        void IGameVisual.OnVisualUpdate(PvSoUnitAbility ability, GridPawnUnit unit)
        {
            UpdateHoverCells(unit, ability);
        }
        
        void IGameVisual.OnVisualUpdate(GridPawnUnit unit)
        {
            UpdateHoverCells(unit);
        }

        public void ResetVisualStateCells()
        {
            foreach (LevelCellBase editedCell in _bufferedVisualStateCells)
            {
                TacticBattleManager.ResetCellState(editedCell);
            }

            _bufferedVisualStateCells.Clear();
        }
        
        /// <summary>
        /// Highlight Ability Range, but you need do reset manually
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="casterUnit"></param>
        public void HighlightAbilityRange(PvSoUnitAbility ability, GridPawnUnit casterUnit)
        {
            if (!ability.GetShape())
            {
                return;
            }

            List<LevelCellBase> abilityCells = ability.GetAbilityCells(casterUnit);

            CellState abilityState = ability.GetEffectedTeam() == BattleTeam.Hostile ? CellState.eNegative : CellState.ePositive;

            foreach (LevelCellBase cell in abilityCells)
            {
                _bufferedVisualStateCells.Add(cell);
                TacticBattleManager.SetCellState(cell, abilityState);
            }
        }

        public bool ResetAndHighlightMovementRange(GridPawnUnit casterUnit)
        {
            if (casterUnit.IsMoving())
            {
                return false;
            }

            ResetVisualStateCells();
            List<LevelCellBase> allowedMovementCells = casterUnit.GetAllowedMovementCells();
            foreach (LevelCellBase cell in allowedMovementCells)
            {
                if (cell && cell.IsVisible())
                {
                    _bufferedVisualStateCells.Add(cell);
                    TacticBattleManager.SetCellState(cell, CellState.eMovement);
                }
            }

            return true;
        }

        private void UpdateHoverCells(GridPawnUnit selectedUnit)
        {
            if (!selectedUnit)
            {
                throw new NullReferenceException("ERROR: No Unit is selected!");
            }

            CleanupHoverCells();

            if (CurrentHoverCell)
            {
                _hoveringCells.Add(CurrentHoverCell);

                if (selectedUnit)
                {
                    List<LevelCellBase> allowedMovementCells = selectedUnit.GetAllowedMovementCells();

                    if (allowedMovementCells.Contains(CurrentHoverCell))
                    {
                        List<LevelCellBase> pathToCursor =
                            selectedUnit.GetPathTo(CurrentHoverCell, allowedMovementCells);

                        foreach (LevelCellBase pathCell in pathToCursor)
                        {
                            if (pathCell && pathCell != CurrentHoverCell)
                            {
                                _hoveringCells.Add(pathCell);
                                // if (allowedMovementCells.Contains(pathCell)) // Normally this won't happen
                            }
                        }
                    }
                }
                
                RefreshHoveringVisualCells();
            }
        }

        private void UpdateHoverCells(GridPawnUnit selectedUnit, PvSoUnitAbility ability)
        {
            if (!selectedUnit)
            {
                throw new NullReferenceException("ERROR: No Unit is selected!");
            }
            CleanupHoverCells();

            if (CurrentHoverCell)
            {
                // GridPawnUnit hoverUnit = CurrentHoverCell.GetUnitOnCell();
                // TODO: Add Event if necessary

                _hoveringCells.Add(CurrentHoverCell);
                UpdateHoverCellsToList(selectedUnit, ability, CurrentHoverCell);
                RefreshHoveringVisualCells();
            }
        }
        
        private void UpdateHoverCells()
        {
            CleanupHoverCells();

            if (CurrentHoverCell)
            {
                // GridPawnUnit hoverUnit = CurrentHoverCell.GetUnitOnCell();
                // TODO: Add Event if necessary

                _hoveringCells.Add(CurrentHoverCell);
                RefreshHoveringVisualCells();
            }
        }

        private void RefreshHoveringVisualCells()
        {
            foreach (LevelCellBase currCell in _hoveringCells)
            {
                currCell.SetMaterial(CellState.eHover);
            }

            CurrentHoverCell.HandleMouseOver();
        }

        public void BeginHover(LevelCellBase cell)
        {
            CurrentHoverCell = cell;
            UpdateHoverCells();
        }
        
        public void EndHover(LevelCellBase cell)
        {
            CleanupHoverCells();

            if (cell)
            {
                cell.HandleMouseExit();
            }

            CurrentHoverCell = null;
            // TODO: EndHover Event if necessary
        }

        private void CleanupHoverCells()
        {
            foreach (var currCell in _hoveringCells)
            {
                if (currCell)
                {
                    currCell.SetMaterial(currCell.GetCellState());
                }
            }

            _hoveringCells.Clear();
        }
        
        private void UpdateHoverCellsToList(GridPawnUnit caster, PvSoUnitAbility ability, LevelCellBase cell)
        {
            if (!ability)
            {
                throw new NullReferenceException("ERROR: Cannot identify current Ability");
            }
            
            if (ability)
            {
                List<LevelCellBase> abilityCells = ability.GetAbilityCells(caster);
                List<LevelCellBase> effectedCells = ability.GetEffectedCells(caster, cell);

                if (abilityCells.Contains(cell))
                {
                    foreach (var currCell in effectedCells)
                    {
                        if (!currCell || currCell == cell)
                        {
                            continue;
                        }

                        if (TacticBattleManager.CanCasterEffectTarget(caster.GetCell(), currCell, BattleTeam.All,
                                ability.DoesAllowBlocked()))
                        {
                            _hoveringCells.Add(currCell);
                        }
                    }
                }
            }
        }
    }
}