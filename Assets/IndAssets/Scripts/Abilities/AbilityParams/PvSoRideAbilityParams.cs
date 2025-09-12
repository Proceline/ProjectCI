using System;
using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoRideParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoRideParams")]
    public class PvSoRideAbilityParams : AbilityParamBase
    {
        public const string RideCommandResultTag = "ToRide";
        public AttributeType carryOverAttribute;

        public override string GetAbilityInfo()
        {
            // TODO: Description
            return base.GetAbilityInfo();
        }

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, List<CommandResult> results)
        {
            
            var toContainer = toUnit.RuntimeAttributes;
            var fromContainer = fromUnit.RuntimeAttributes;
            
            int fromCarryValue = fromContainer.GetAttributeValue(carryOverAttribute);
            int toCarryValue = toContainer.GetAttributeValue(carryOverAttribute);
            var delta = fromCarryValue - toCarryValue;

            if (delta == 0)
            {
                throw new InvalidOperationException("ERROR: Same size should not be able to ride");
            }
            results.Add(new PvRideCommand
            {
                ResultId = resultId,
                AbilityId = ability.ID,
                OwnerId = fromUnit.ID,
                TargetCellIndex = toUnit.GetCell().GetIndex(),
                Value = delta,
                CommandType = RideCommandResultTag
            });
        }
    }
}
