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

        private void HighlightAbilityOrSupportCells(PvSoUnitAbility ability, PvMnBattleGeneralUnit unit, CellState state)
        {
            List<LevelCellBase> abilityCells = ability.GetAbilityCells(unit);
            foreach (LevelCellBase cell in abilityCells)
            {
                _bufferedVisualStateCells.Add(cell);
                TacticBattleManager.SetCellState(cell, state);
            }
        }

        private void HighlightMovementRange(GridPawnUnit casterUnit)
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
                    var ability = unit.EquippedAbility;
                    HighlightAbilityAndSupportRange(unit);
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
                case UnitBattleState.AbilityTargeting:
                case UnitBattleState.Idle:
                case UnitBattleState.AbilityConfirming:
                default:
                    // Empty
                    break;
            }
        }

        /// <summary>
        /// Both used in Event Call and Internal
        /// </summary>
        public void HighlightAbilityAndSupportRange(PvMnBattleGeneralUnit casterUnit)
        {
            ResetVisualStateCells();
            var ability = casterUnit.EquippedAbility;
            if (!ability || !ability.GetShape())
            {
                throw new NullReferenceException("ERROR: Ability MUST have Shape!");
            }

            HighlightAbilityOrSupportCells(ability, casterUnit, CellState.eNegative);

            var support = casterUnit.DefaultSupport;
            if (!support.GetShape())
            {
                throw new NullReferenceException("ERROR: Support MUST have Shape!");
            }

            HighlightAbilityOrSupportCells(support, casterUnit, CellState.ePositive);
        }
        
        public void ReHighlightAbilityRange(PvSoUnitAbility ability, PvMnBattleGeneralUnit casterUnit)
        {
            ResetVisualStateCells();

            if (!ability)
            {
                return;
            }

            if (!ability.GetShape())
            {
                throw new NullReferenceException("ERROR: This specific Ability MUST have Shape!");
            }

            HighlightAbilityOrSupportCells(ability, casterUnit,
                ability.GetEffectedTeam() == BattleTeam.Friendly ? CellState.ePositive : CellState.eNegative);
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