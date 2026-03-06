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
    [CreateAssetMenu(fileName = "PvSoEnergyAdjustAbilityParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoEnergyAdjustAbilityParams")]
    public class PvSoEnergyAdjustAbilityParams : AbilityParamBase
    {
        [SerializeField]
        private AttributeType energyValueDependency;

        [SerializeField]
        private bool typeOnReceiver;

        [SerializeField]
        private float threshold = 1f;

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase currentTargetCell, Queue<CommandResult> results, int passValue, params uint[] extraInfos)
        {
            var targetUnit = currentTargetCell.GetUnitOnCell();
            if (targetUnit.IsDead() || fromUnit.IsDead())
            {
                return;
            }

            var fromContainer = fromUnit.RuntimeAttributes;
            var toContainer = targetUnit.RuntimeAttributes;

            var energyDeltaValue = typeOnReceiver ? 
                toContainer.GetAttributeValue(energyValueDependency) 
                : fromContainer.GetAttributeValue(energyValueDependency);

            var finalValue = Mathf.FloorToInt(energyDeltaValue * threshold);

            PvEnergyObtainCommand.AdjustAndEnqueueEnergy(resultId, targetUnit.ID, toContainer, finalValue, results);
        }

        public override int MockValue(GridPawnUnit fromUnit, GridPawnUnit targetUnit, uint extraDamageForm)
        {
            return 0;
        }
    }
}
