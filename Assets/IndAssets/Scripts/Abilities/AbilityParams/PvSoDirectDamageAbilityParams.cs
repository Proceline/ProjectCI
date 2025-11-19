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

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "PvSoDirectDamageParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoDirectDamageParams")]
    public class PvSoDirectDamageAbilityParams : AbilityParamBase
    {
        public AttributeType attackerAttribute;
        public AttributeType defenderAttribute;
        public PvEnDamageType damageType;

        [SerializeField]
        private bool isHealValue;

        [SerializeField] private int basicAddon;
        
        [Header("Critical")]
        [SerializeField]
        private bool isCriticalEnabledByDefault;

        [Header("Accuracy")] 
        [SerializeField] 
        private bool isAlwaysHitByDefault;

        [Inject]
        private static IFinalReceiveDamageModifier _receiveDamageModifier;

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase currentTargetCell, Queue<CommandResult> results, int passValue)
        {
            var targetUnit = currentTargetCell.GetUnitOnCell();
            if (!targetUnit)
            {
                return;
            }

            var toContainer = targetUnit.RuntimeAttributes;
            var fromContainer = fromUnit.RuntimeAttributes;

            if (targetUnit.IsDead())
            {
                return;
            }

            var beforeHealth = toContainer.Health.CurrentValue;
            var damage = fromContainer.GetAttributeValue(attackerAttribute) + basicAddon;

            var isReallyHit = isAlwaysHitByDefault || passValue > 0;

            var isCritical = false;
            if (isCriticalEnabledByDefault && passValue == 100)
            {
                damage *= 2;
                isCritical = true;
            }

            var deltaDamage =
                isReallyHit ? Mathf.Max(damage - toContainer.GetAttributeValue(defenderAttribute), 0) : 0;

            var finalDeltaDamage = deltaDamage;
            if (targetUnit is IEventOwner damageReceiver)
            {
                finalDeltaDamage = _receiveDamageModifier.CalculateResult(damageReceiver, deltaDamage);
            }

            if (isReallyHit)
            {
                if (!isHealValue)
                {
                    toContainer.Health.ModifyValue(-finalDeltaDamage);
                }
                else
                {
                    toContainer.Health.ModifyValue(finalDeltaDamage);
                }
            }

            var afterHealth = toContainer.Health.CurrentValue;

            var savingCommand = new PvSimpleDamageCommand
            {
                ResultId = resultId,
                AbilityId = ability.ID,
                OwnerId = fromUnit.ID,
                TargetCellIndex = targetUnit.GetCell().GetIndex(),
                BeforeValue = beforeHealth,
                AfterValue = afterHealth,
                Value = finalDeltaDamage,
                DamageType = damageType
            };

            if (isHealValue)
            {
                savingCommand.ExtraInfo = UnitAbilityCoreExtensions.HealExtraInfoHint;
            }
            else if (!isReallyHit)
            {
                savingCommand.ExtraInfo = UnitAbilityCoreExtensions.MissExtraInfoHint;
            }
            else if (isCritical && finalDeltaDamage > 0)
            {
                savingCommand.ExtraInfo = UnitAbilityCoreExtensions.CriticalExtraInfoHint;
            }

            results.Enqueue(savingCommand);

            // Add Die Command if Health is 0
            if (!targetUnit.IsDead())
            {
                return;
            }

            Debug.Log($"<color=red>{targetUnit.name} is Dead!</color>");

            var dieCommand = new PvDieCommand
            {
                ResultId = resultId,
                AbilityId = ability.ID,
                OwnerId = targetUnit.ID,
                TargetCellIndex = targetUnit.GetCell().GetIndex()
            };

            results.Enqueue(dieCommand);

        }
    }
}
