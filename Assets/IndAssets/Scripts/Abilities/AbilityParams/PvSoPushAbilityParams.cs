using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoPushParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoPushParams")]
    public class PvSoPushAbilityParams : AbilityParamBase
    {
        [SerializeField] private int pushDistance;

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, LevelCellBase currentTarget, Queue<CommandResult> results, int passValue, params uint[] extraInfos)
        {
            if (currentTarget != toUnit.GetCell())
            {
                return;
            }
            
            if (toUnit.IsDead())
            {
                return;
            }

            if (passValue <= 0)
            {
                return;
            }

            var pushCommand = new PvPushCommand
            {
                ResultId = resultId,
                OwnerId = fromUnit.ID,
                TargetCellIndex = currentTarget.GetIndex(),
                Distance = pushDistance,
                FromCell = fromUnit.GetCell()
            };

            results.Enqueue(pushCommand);
        }

        public override int MockValue(GridPawnUnit fromUnit, GridPawnUnit targetUnit, uint damageForm)
        {
            // Empty;
            return 0;
        }
    }
}
