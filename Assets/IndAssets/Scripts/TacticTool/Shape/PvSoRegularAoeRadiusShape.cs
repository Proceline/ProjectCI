using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Concrete
{

    [CreateAssetMenu(fileName = "PvSoRegularAoeRadiusShape", menuName = "ProjectCI Tools/Ability/Shapes/PvSoRegularAoeRadiusShape")]
    public class PvSoRegularAoeRadiusShape : AbilityShape
    {
        [SerializeField] private bool onlyIncludedTargets = true;
        [SerializeField] private bool isSquare = true;
        [SerializeField] private BattleTeam customEffectedTeam = BattleTeam.Hostile;

        public override List<LevelCellBase> GetCellList(GridPawnUnit caster, LevelCellBase cell, int range,
            bool allowBlocked = true, BattleTeam effectedTeam = BattleTeam.None)
        {
            var cellIndex = cell.GetIndex();
            var cells = new List<LevelCellBase>();
            var grid = TacticBattleManager.GetGrid();

            for (var i = -range; i <= range; i++)
            {
                for (var j = -range; j <= range; j++)
                {
                    if (!isSquare && (Mathf.Abs(i) + Mathf.Abs(j)) > range)
                    {
                        continue;
                    }

                    var possibleCell = grid[cellIndex.x + i, cellIndex.y + j];
                    if (possibleCell)
                    {
                        cells.Add(possibleCell);
                    }
                }
            }

            if (onlyIncludedTargets)
            {
                for (var i = cells.Count - 1; i >= 0; i--)
                {
                    var currCell = cells[i];
                    GridPawnUnit unitOnCell = currCell.GetUnitOnCell();
                    if (unitOnCell)
                    {
                        BattleTeam relationToCaster =
                            TacticBattleManager.GetTeamAffinity(caster.GetTeam(), unitOnCell.GetTeam());
                        if (relationToCaster != customEffectedTeam)
                        {
                            cells.RemoveAt(i);
                        }
                    }
                    else
                    {
                        cells.RemoveAt(i);
                    }
                }
            }
            
            return cells;
        }
    }
}
