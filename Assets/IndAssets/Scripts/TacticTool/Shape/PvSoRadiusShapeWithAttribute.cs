using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Concrete
{

    [CreateAssetMenu(fileName = "PvSoRadiusShapeWithAttribute", menuName = "ProjectCI Tools/Ability/Shapes/PvSoRadiusShapeWithAttribute")]
    public class PvSoRadiusShapeWithAttribute : AbilityShape
    {
        [SerializeField] private bool onlyIncludedTargets = true;
        [SerializeField] private bool allowStopOnBlock;
        [SerializeField] private AttributeType boostAttribute;
        [SerializeField] private float threshold = 1f;
        [SerializeField] private bool sumOrDifference = true; // true = sum, false = difference

        public override List<LevelCellBase> GetCellList(GridPawnUnit caster, LevelCellBase cell, int range,
            bool allowBlocked = true, BattleTeam effectedTeam = BattleTeam.None)
        {
            var attributeValue = (int)(caster.RuntimeAttributes.GetAttributeValue(boostAttribute) * threshold);
            var realRange = range;
            if (sumOrDifference)
            {
                range += attributeValue;
            }
            else
            {
                range -= attributeValue;
            }
            
            if (range < 1)
            {
                range = 1;
            }

            AIRadiusInfo radiusInfo = new AIRadiusInfo(cell, range)
            {
                Caster = caster,
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
