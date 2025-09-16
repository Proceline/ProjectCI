using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Concrete
{
    /// <summary>
    /// 四边形Grid上的十字范围
    /// </summary>

    [CreateAssetMenu(fileName = "PvSoDirectionalShape", menuName = "ProjectCI Tools/Ability/Shapes/PvSoDirectionalShape")]
    public class PvSoDirectionalShape : AbilityShape
    {
        [SerializeField] private bool onlyIncludedTargets = true;

        public override List<LevelCellBase> GetCellList(GridPawnUnit caster, LevelCellBase cell, int range,
            bool allowBlocked = true, BattleTeam effectedTeam = BattleTeam.None)
        {
            List<LevelCellBase> cells = new();

            if (cell && range > 0)
            {
                cells.AddRange(GetCellsInDirection(cell, range, CompassDir.N, allowBlocked, effectedTeam));
                cells.AddRange(GetCellsInDirection(cell, range, CompassDir.E, allowBlocked, effectedTeam));
                cells.AddRange(GetCellsInDirection(cell, range, CompassDir.NE, allowBlocked, effectedTeam));
                cells.AddRange(GetCellsInDirection(cell, range, CompassDir.NW, allowBlocked, effectedTeam));
                cells.AddRange(GetCellsInDirection(cell, range, CompassDir.SE, allowBlocked, effectedTeam));
                cells.AddRange(GetCellsInDirection(cell, range, CompassDir.S, allowBlocked, effectedTeam));
                cells.AddRange(GetCellsInDirection(cell, range, CompassDir.SW, allowBlocked, effectedTeam));
                cells.AddRange(GetCellsInDirection(cell, range, CompassDir.W, allowBlocked, effectedTeam));
            }

            // if (onlyIncludedTargets)
            // {
            //     for (var i = cells.Count - 1; i >= 0; i--)
            //     {
            //         var currCell = cells[i];
            //         GridPawnUnit unitOnCell = currCell.GetUnitOnCell();
            //         if (unitOnCell)
            //         {
            //             var relationToCaster = TacticBattleManager.GetTeamAffinity(caster.GetTeam(), unitOnCell.GetTeam());
            //             if (relationToCaster != effectedTeam)
            //             {
            //                 cells.RemoveAt(i);
            //             }
            //         }
            //     }
            // }

            return cells;
        }

        List<LevelCellBase> GetCellsInDirection(LevelCellBase startCell, int range, CompassDir dir, bool bAllowBlocked,
            BattleTeam effectedTeam)
        {
            List<LevelCellBase> cells = new();

            if (range <= 0)
            {
                return cells;
            }

            LevelCellBase cursorCell = startCell.GetAdjacentCell(dir);

            int lengthCount = 0;
            while (cursorCell)
            {
                if (cursorCell.IsBlocked() && !bAllowBlocked)
                {
                    break;
                }

                if (onlyIncludedTargets)
                {
                    var gridObj = cursorCell.GetObjectOnCell();
                    if (gridObj)
                    {
                        if (effectedTeam == BattleTeam.None)
                        {
                            break;
                        }

                        var relationToCaster =
                            TacticBattleManager.GetTeamAffinity(gridObj.GetTeam(), startCell.GetCellTeam());
                        if (relationToCaster == effectedTeam)
                        {
                            cells.Add(cursorCell);
                        }
                    }
                }
                else
                {
                    cells.Add(cursorCell);
                }

                cursorCell = cursorCell.GetAdjacentCell(dir);
                lengthCount++;
                if (lengthCount >= range)
                {
                    break;
                }
            }

            return cells;
        }
    }
}
