using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Concrete
{

    [CreateAssetMenu(fileName = "PvSoRadiusShape", menuName = "ProjectCI Tools/Ability/Shapes/PvSoRadiusShape")]
    public class PvSoRadiusShape : AbilityShape
    {
        [SerializeField] private bool onlyIncludedTargets = true;
        [SerializeField] private bool allowStopOnBlock;

        public override List<LevelCellBase> GetCellList(GridPawnUnit caster, LevelCellBase cell, int range,
            bool allowBlocked = true, BattleTeam effectedTeam = BattleTeam.None)
        {
            AIRadiusInfo radiusInfo = new AIRadiusInfo(cell, range)
            {
                Caster = cell.GetUnitOnCell(),
                bAllowBlocked = allowBlocked,
                bStopAtBlockedCell = allowStopOnBlock,
                EffectedTeam = effectedTeam
            };

            var radCells = AStarAlgorithmUtils.GetRadius(radiusInfo);

            if (onlyIncludedTargets)
            {
                List<LevelCellBase> cells = new List<LevelCellBase>();
                foreach (var currCell in radCells)
                {
                    GridPawnUnit unitOnCell = currCell.GetUnitOnCell();
                    if (unitOnCell)
                    {
                        BattleTeam relationToCaster =
                            TacticBattleManager.GetTeamAffinity(caster.GetTeam(), unitOnCell.GetTeam());
                        if (relationToCaster == effectedTeam)
                        {
                            cells.Add(currCell);
                        }
                    }
                }

                return cells;
            }
            
            return radCells;
        }
    }
}
