using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.Utilities.Runtime.Modifiers;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "PvSoDirectDamageParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoDirectDamageParams")]
    public class PvSoDirectDamageAbilityParams : AbilityParamBase
    {
        // TODO: This will affect player's attack might
        public int m_Damage;

        public AttributeType attackerAttribute;
        public AttributeType defenderAttribute;

        [Inject, NonSerialized]
        private IFinalReceiveDamageModifier _receiveDamageModifier;

        protected IFinalReceiveDamageModifier ReceiveDamageModifier
        {
            get
            {
                if (_receiveDamageModifier == null)
                {
                    DIConfiguration.InjectFromConfiguration(this);
                }

                return _receiveDamageModifier;
            }
        }

        public override string GetAbilityInfo()
        {
            // TODO: Description
            return base.GetAbilityInfo();
        }

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit, GridPawnUnit toUnit, List<CommandResult> results)
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
            
            if (results != null)
            {
                results.Add(new CommandDamageResult
                {
                    ResultId = resultId,
                    AbilityId = ability.ID,
                    OwnerId = fromUnit.ID,
                    TargetCellIndex = toUnit.GetCell().GetIndex(),
                    BeforeValue = beforeHealth,
                    AfterValue = afterHealth,
                    CommandType = CommandResult.TakeDamage,
                    Value = deltaDamage,
                    ExtraInfo = nameof(UnitAttributeContainer.Health)
                });
            }
        }
    }
}
