using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace IndAssets.Scripts.AI
{
    public class PvMnEnemyUnitThought : MonoBehaviour
    {
        public void ShowTargetPawnField(PvMnBattleGeneralUnit inSceneUnit, bool allowBlock, bool toTeammate)
        {
            var startUnit = inSceneUnit;
            if (!startUnit)
            {
                Debug.LogError("You must drag and drop a unit!");
                return;
            }

            var movementPoint = startUnit.GetCurrentMovementPoints();
            
            AIRadiusInfo radiusInfo = new AIRadiusInfo(startUnit.GetCell(), movementPoint)
            {
                Caster = startUnit,
                bAllowBlocked = allowBlock,
                bStopAtBlockedCell = true,
                EffectedTeam = BattleTeam.Friendly
            };

            var radiusField = BucketDijkstraSolutionUtils.CalculateBucket(radiusInfo, false, 10);

            foreach (var pair in radiusField.Dist)
            {
                var cell = pair.Key;
                var value = pair.Value;
            }

            var ability = startUnit.EquippedAbility;
            if (ability)
            {
                var attackField = BucketDijkstraSolutionUtils.ComputeAttackField(radiusField, GetCellList);

                List<LevelCellBase> GetCellList(LevelCellBase startCell)
                {
                    return ability.GetShape().GetCellList(startUnit, startCell, ability.GetRadius(),
                        ability.DoesAllowBlocked(), ability.GetEffectedTeam());
                }

                var victimDic = attackField.VictimsFromCells;
            }
        }
    }
}