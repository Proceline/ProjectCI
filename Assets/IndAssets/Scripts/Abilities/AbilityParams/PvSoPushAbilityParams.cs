using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoPushParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoPushParams")]
    public class PvSoPushAbilityParams : AbilityParamBase
    {
        [SerializeField] private int defaultDistance = 1;
        [SerializeField] private AttributeType boostAttribute;
        [SerializeField] private float threshold = 1f;

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

            var finalDistance = defaultDistance;

            if (threshold > 0.01f)
            {
                var attributeValue = (int)(fromUnit.RuntimeAttributes.GetAttributeValue(boostAttribute) * threshold);
                finalDistance += attributeValue;
            }

            PvPushCommand.ExecuteAndAddPushCommand(resultId, fromUnit, toUnit, finalDistance, currentTarget, results);
        }

        public override int MockValue(GridPawnUnit fromUnit, GridPawnUnit targetUnit, uint damageForm)
        {
            // Empty;
            return 0;
        }
    }
}
