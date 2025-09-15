using System;
using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.Utilities.Runtime.Modifiers;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoDirectDamageParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoDirectDamageParams")]
    public class PvSoDirectDamageAbilityParams : AbilityParamBase
    {
        public AttributeType attackerAttribute;
        public AttributeType defenderAttribute;
        public PvEnDamageType damageType;

        [Inject, NonSerialized]
        private static IFinalReceiveDamageModifier _receiveDamageModifier;

        [NonSerialized]
        private static bool _modifierInjected;

        private IFinalReceiveDamageModifier ReceiveDamageModifier
        {
            get
            {
                if (_modifierInjected)
                {
                    return _receiveDamageModifier;
                }

                DIConfiguration.InjectFromConfiguration(this);
                _modifierInjected = true;

                return _receiveDamageModifier;
            }
        }

        public override string GetAbilityInfo()
        {
            // TODO: Description
            return base.GetAbilityInfo();
        }

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, Queue<CommandResult> results)
        {
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

                var deltaDamage =
                    Mathf.Max(damage - toContainer.GetAttributeValue(defenderAttribute), 0);

                var finalDeltaDamage = deltaDamage;
                if (ReceiveDamageModifier != null && targetUnit is IEventOwner damageReceiver)
                {
                    finalDeltaDamage = ReceiveDamageModifier.CalculateResult(damageReceiver, deltaDamage);
                }

                toContainer.Health.ModifyValue(-finalDeltaDamage);
                int afterHealth = toContainer.Health.CurrentValue;

                results.Enqueue(new PvSimpleDamageCommand
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
                });
            }
        }
    }
}
