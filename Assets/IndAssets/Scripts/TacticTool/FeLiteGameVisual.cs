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
    public class FeLiteGameVisual : ScriptableObject
    {
        internal LevelCellBase CurrentHoverCell { get; private set; }
        private readonly List<LevelCellBase> _bufferedVisualStateCells = new();
        private readonly List<LevelCellBase> _hoveringCells = new();

        [NonSerialized] 
        private GameObject _pawnVisualMark;

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

        public void HighlightMovementRange(GridPawnUnit casterUnit)
        {
            List<LevelCellBase> allowedMovementCells = casterUnit.GetAllowedMovementCells();
            foreach (LevelCellBase cell in allowedMovementCells)
            {
                if (cell && cell.IsVisible())
                {
                    _bufferedVisualStateCells.Add(cell);
                    TacticBattleManager.SetCellState(cell, CellState.eMovement);
                }
            }
        }

        public void UpdateHoverCells(PvMnBattleGeneralUnit selectedUnit)
        {
            CleanupHoverCells();

            if (!CurrentHoverCell)
            {
                return;
            }
            // GridPawnUnit hoverUnit = CurrentHoverCell.GetUnitOnCell();
            // TODO: Add Event if necessary

            _hoveringCells.Add(CurrentHoverCell);
            if (!selectedUnit)
            {
                RefreshHoveringVisualCells();
                return;
            }

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

            RefreshHoveringVisualCells();
        }

        public void UpdateHoverCells(PvMnBattleGeneralUnit selectedUnit, PvSoUnitAbility ability)
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
            UpdateHoverCells(null);
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
        
        #region MethodsCombo
        public void ShowRangeWhileStateChanged(PvMnBattleGeneralUnit unit, UnitBattleState state)
        {
            switch (state)
            {
                case UnitBattleState.UsingAbility:
                case UnitBattleState.AbilityTargeting:
                    var ability = unit.EquippedAbility;
                    ResetVisualStateCells();
                    HighlightAbilityRange(ability, unit);
                    //TODO: Consider ChangeStateForSelectedUnit(UnitBattleState.AbilityTargeting);
                    UpdateHoverCells(unit, ability);
                    break;
                case UnitBattleState.Finished:
                    ResetVisualStateCells();
                    break;
                case UnitBattleState.Moving:
                    ResetVisualStateCells();
                    HighlightMovementRange(unit);
                    break;
                case UnitBattleState.MovingProgress:
                    ResetVisualStateCells();
                    break;
                case UnitBattleState.Idle:
                case UnitBattleState.AbilityConfirming:
                default:
                    // Empty
                    break;
            }
        }
        #endregion
        
        #region NonGrid Visual

        public void ShowPawnMarker(PvMnBattleGeneralUnit selectedUnit)
        {
            if (!_pawnVisualMark)
            {
                GameObject markPrefab = TacticBattleManager.GetSelectedHoverPrefab();
                _pawnVisualMark = Instantiate(markPrefab);
            }

            _pawnVisualMark.SetActive(false);

            if (!selectedUnit) return;
            _pawnVisualMark.transform.position = selectedUnit.GetCell().GetAllignPos(selectedUnit);
            _pawnVisualMark.transform.SetParent(selectedUnit.transform);
            _pawnVisualMark.SetActive(true);
        }

        public void HidePawnMarker()
        {
            if (!_pawnVisualMark) return;
            _pawnVisualMark.SetActive(false);
            _pawnVisualMark.transform.SetParent(null);
        }

        #endregion
    }
}