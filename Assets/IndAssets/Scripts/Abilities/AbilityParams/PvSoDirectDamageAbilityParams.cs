using System;
using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
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
            GridPawnUnit toUnit, List<CommandResult> results)
        {
            var toContainer = toUnit.RuntimeAttributes;
            var fromContainer = fromUnit.RuntimeAttributes;

            int beforeHealth = toContainer.Health.CurrentValue;
            int damage = fromContainer.GetAttributeValue(attackerAttribute);

            int deltaDamage =
                Mathf.Max(damage - toContainer.GetAttributeValue(defenderAttribute), 0);

            int finalDeltaDamage = deltaDamage;
            if (ReceiveDamageModifier != null && toUnit is IEventOwner damageReceiver)
            {
                finalDeltaDamage = ReceiveDamageModifier.CalculateResult(damageReceiver, deltaDamage);
            }

            toContainer.Health.ModifyValue(-finalDeltaDamage);
            int afterHealth = toContainer.Health.CurrentValue;

            results.Add(new PvSimpleDamageCommand
            {
                ResultId = resultId,
                AbilityId = ability.ID,
                OwnerId = fromUnit.ID,
                TargetCellIndex = toUnit.GetCell().GetIndex(),
                BeforeValue = beforeHealth,
                AfterValue = afterHealth,
                CommandType = CommandResult.TakeDamage,
                Value = deltaDamage,
                DamageType = damageType,
                ExtraInfo = nameof(UnitAttributeContainer.Health)
            });
        }
    }
}
