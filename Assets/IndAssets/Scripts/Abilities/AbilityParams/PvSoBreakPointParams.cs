using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoBreakPointParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoBreakPointParams")]
    public class PvSoBreakPointParams : AbilityParamBase
    {
        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase currentTargetCell, Queue<CommandResult> results, int passValue, params uint[] unusedParams)
        {
            var targetUnit = currentTargetCell.GetUnitOnCell();
            if (targetUnit.IsDead() || fromUnit.IsDead())
            {
                return;
            }

            results.Enqueue(new PvBreakPointCommand());
        }

        public override int MockValue(GridPawnUnit fromUnit, GridPawnUnit targetUnit, uint extraDamageForm) => 0;
    }
}
