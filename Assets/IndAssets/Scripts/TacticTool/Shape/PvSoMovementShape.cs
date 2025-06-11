using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Concrete
{

    [CreateAssetMenu(fileName = "PvSoMovementShape", menuName = "ProjectCI Tools/Ability/Shapes/PvSoMovementShape")]
    public class PvSoMovementShape : AbilityShape
    {
        public override List<LevelCellBase> GetCellList(GridPawnUnit caster, LevelCellBase inCell, int inRange,
            bool bAllowBlocked, BattleTeam effectedTeam)
        {
            GridPawnUnit Caster = inCell.GetUnitOnCell();

            AIRadiusInfo radiusInfo = new AIRadiusInfo(inCell, inRange)
            {
                Caster = Caster,
                bAllowBlocked = bAllowBlocked,
                bStopAtBlockedCell = true,
                EffectedTeam = effectedTeam
            };

            return AStarAlgorithmUtils.GetRadius(radiusInfo);
        }
    }
}
