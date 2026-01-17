using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System;
using System.Collections.Generic;
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

        [NonSerialized]
        private List<LevelCellBase> _bufferedAttackCells;
        [NonSerialized]
        private List<LevelCellBase> _bufferedSupportCells;
        [NonSerialized]
        private bool _isShowingActionRange;

        public void AssignAbilityOnView(PvSoUnitAbility ability, PvMnBattleGeneralUnit unit)
        {
            // Empty, TBD, deprecated
        }
        
        public void ResetVisualStateCells()
        {
            foreach (LevelCellBase editedCell in _bufferedVisualStateCells)
            {
                TacticBattleManager.ResetCellState(editedCell);
            }

            _bufferedVisualStateCells.Clear();
        }

        private void ShowUnitGeneralAbilitiesCells(PvMnBattleGeneralUnit unit)
        {
            if (!_isShowingActionRange)
            {
                _bufferedAttackCells = unit.AttackAbility.GetAbilityCells(unit);
                _bufferedSupportCells = unit.SupportAbility.GetAbilityCells(unit);
                _isShowingActionRange = true;
            }

            foreach (LevelCellBase cell in _bufferedAttackCells)
            {
                _bufferedVisualStateCells.Add(cell);
                TacticBattleManager.SetCellState(cell, CellState.eNegative);
            }

            foreach (LevelCellBase cell in _bufferedSupportCells)
            {
                _bufferedVisualStateCells.Add(cell);
                TacticBattleManager.SetCellState(cell, CellState.ePositive);
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

        public void UpdateHoverCellsWithGeneralAbilities(PvMnBattleGeneralUnit selectedUnit)
        {
            if (!selectedUnit)
            {
                throw new NullReferenceException("ERROR: No Unit is selected!");
            }
            CleanupHoverCells();

            if (CurrentHoverCell)
            {
                _hoveringCells.Add(CurrentHoverCell);

                if (!_isShowingActionRange)
                {
                    _bufferedAttackCells = selectedUnit.AttackAbility.GetAbilityCells(selectedUnit);
                    _bufferedSupportCells = selectedUnit.SupportAbility.GetAbilityCells(selectedUnit);
                    _isShowingActionRange = true;
                }

                var cell = CurrentHoverCell;

                var isAttackable = _bufferedAttackCells.Contains(cell);
                if (isAttackable || _bufferedSupportCells.Contains(cell))
                {
                    var ability = isAttackable ? selectedUnit.AttackAbility : selectedUnit.SupportAbility;
                    List<LevelCellBase> effectedCells = ability.GetEffectedCells(selectedUnit, cell);
                    foreach (var currCell in effectedCells)
                    {
                        if (!currCell || currCell == cell)
                        {
                            continue;
                        }

                        if (TacticBattleManager.CanCasterEffectTarget(selectedUnit.GetCell(), currCell, BattleTeam.All,
                                ability.DoesAllowBlocked()))
                        {
                            _hoveringCells.Add(currCell);
                        }
                    }
                }

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

        #region MethodsCombo
        public void ShowRangeWhileStateChanged(PvMnBattleGeneralUnit unit, UnitBattleState state)
        {
            switch (state)
            {
                //case UnitBattleState.UsingAbility:
                case UnitBattleState.AbilityTargeting:
                    _isShowingActionRange = false;
                    HighlightAbilityAndSupportRange(unit);
                    UpdateHoverCellsWithGeneralAbilities(unit);
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

        /// <summary>
        /// Both used in Event Call and Internal
        /// </summary>
        private void HighlightAbilityAndSupportRange(PvMnBattleGeneralUnit casterUnit)
        {
            ResetVisualStateCells();
            ShowUnitGeneralAbilitiesCells(casterUnit);
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
            _pawnVisualMark.transform.position = selectedUnit.GetCell().GetAlignPos(selectedUnit);
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