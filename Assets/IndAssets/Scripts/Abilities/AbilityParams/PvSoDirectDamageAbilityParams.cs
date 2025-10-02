using System;
using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.Utilities.Runtime.Modifiers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "PvSoDirectDamageParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoDirectDamageParams")]
    public class PvSoDirectDamageAbilityParams : AbilityParamBase
    {
        public AttributeType attackerAttribute;
        public AttributeType defenderAttribute;
        public PvEnDamageType damageType;
        
        [Header("Critical")]
        [SerializeField]
        private bool isCriticalEnabledByDefault;
        [SerializeField]
        private AttributeType criticalAttribute;

        [Inject]
        private static IFinalReceiveDamageModifier _receiveDamageModifier;

        public override string GetAbilityInfo()
        {
            // TODO: Description
            return base.GetAbilityInfo();
        }

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, Queue<CommandResult> results)
        {
            // TODO: Consider targetCell as Main Target
            var targetCell = toUnit.GetCell();
            List<LevelCellBase> effectedCells = ability.GetEffectedCells(fromUnit, toUnit.GetCell());

            foreach (var cell in effectedCells)
            {
                var targetUnit = cell.GetUnitOnCell();
                if (!targetUnit)
                {
                    continue;
                }

                var toContainer = targetUnit.RuntimeAttributes;
                var fromContainer = fromUnit.RuntimeAttributes;

                var beforeHealth = toContainer.Health.CurrentValue;
                var damage = fromContainer.GetAttributeValue(attackerAttribute);

                bool isCritical = false;
                if (isCriticalEnabledByDefault)
                {
                    var criticalThreshold = fromContainer.GetAttributeValue(criticalAttribute);
                    var finalCriticalThreshold = criticalThreshold + 70;
                    var randomValue = Random.Range(0, 10000) % 100;
                    if (randomValue < finalCriticalThreshold)
                    {
                        damage += 5;
                        damage *= 3;
                        isCritical = true;
                    }
                }

                var deltaDamage =
                    Mathf.Max(damage - toContainer.GetAttributeValue(defenderAttribute), 0);

                var finalDeltaDamage = deltaDamage;
                if (targetUnit is IEventOwner damageReceiver)
                {
                    finalDeltaDamage = _receiveDamageModifier.CalculateResult(damageReceiver, deltaDamage);
                }

                toContainer.Health.ModifyValue(-finalDeltaDamage);
                int afterHealth = toContainer.Health.CurrentValue;

                var savingCommand = new PvSimpleDamageCommand
                {
                    ResultId = resultId,
                    AbilityId = ability.ID,
                    OwnerId = fromUnit.ID,
                    TargetCellIndex = targetUnit.GetCell().GetIndex(),
                    BeforeValue = beforeHealth,
                    AfterValue = afterHealth,
                    CommandType = CommandResult.TakeDamage,
                    Value = deltaDamage,
                    DamageType = damageType
                };

                if (isCritical)
                {
                    savingCommand.ExtraInfo = UnitAbilityCoreExtensions.CriticalExtraInfoHint;
                }

                results.Enqueue(savingCommand);
            }
        }
    }
}
