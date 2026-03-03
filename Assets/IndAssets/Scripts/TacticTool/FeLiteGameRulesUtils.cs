using System.Collections.Generic;
using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.Events;
using IndAssets.Scripts.Managers;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        [Header("Movement Utils")]
        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit, List<LevelCellBase>> onPathDeterminedSupport;

        public void OnTeamRoundEndResponse(BattleTeam team)
        {
            var allUnitsInBattle = unitIdsToBattleUnitHash.Values;
            foreach (var unit in allUnitsInBattle)
            {
                if (unit.IsDead()) continue;
            }

            CurrentTeam = team == BattleTeam.Friendly ? BattleTeam.Hostile : BattleTeam.Friendly;
            BeginTeamTurn(CurrentTeam);
        }

        /// <summary>
        /// Confirm cell target manually (Player Input) based on current battle state
        /// Binded in Mono game controller
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="cellState">If cell state is wrong, it will be ignored</param>
        public void ConfirmCellTarget(LevelCellBase cell, CellState cellState)
        {
            if (!cell)
            {
                return;
            }

            switch (gameBattleState.GetCurrentState)
            {
                case PvPlayerRoundState.None:
                    ApplyCellUnitToSelectedUnit(cell);
                    break;
                case PvPlayerRoundState.Prepare:
                    if (cellState != CellState.ePositive && cellState != CellState.eNegative &&
                        cellState != CellState.eSpecial)
                    {
                        return;
                    }
                    ApplyAbilityToTargetCell(cell, cellState);
                    break;
                case PvPlayerRoundState.Selected:
                    if (cellState != CellState.eMovement && cellState != CellState.eSpecial)
                    {
                        return;
                    }
                    ApplyMovementToCellForSelectedUnit(cell);
                    break;
            }
        }

        /// <summary>
        /// This function is manually binded into Controller
        /// </summary>
        public void CancelCurrentAction()
        {
            if (!_selectedUnit)
            {
                return;
            }

            var playingUnit = _selectedUnit;
            gameBattleState.CancelState(playingUnit);
        }
    }
}