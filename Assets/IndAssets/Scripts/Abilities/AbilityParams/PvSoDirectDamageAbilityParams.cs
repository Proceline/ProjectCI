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

        [SerializeField]
        private bool isHealValue;

        [SerializeField] private int basicAddon;
        
        [Header("Critical")]
        [SerializeField]
        private bool isCriticalEnabledByDefault;
        [SerializeField]
        private AttributeType criticalAttribute;

        [Header("Accuracy")] 
        [SerializeField] 
        private bool isAlwaysHitByDefault;
        [SerializeField] private AttributeType hitAttribute;
        [SerializeField] private AttributeType dodgeAttribute;

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

                if (targetUnit.IsDead())
                {
                    continue;
                }

                var beforeHealth = toContainer.Health.CurrentValue;
                var damage = fromContainer.GetAttributeValue(attackerAttribute) + basicAddon;

                bool isReallyHit = isAlwaysHitByDefault;
                if (!isAlwaysHitByDefault)
                {
                    var hitThreshold = fromContainer.GetAttributeValue(hitAttribute);
                    var dodgeThreshold = toContainer.GetAttributeValue(dodgeAttribute);
                    var hitPercentageResult = hitThreshold - dodgeThreshold;
                    if (hitPercentageResult >= 100)
                    {
                        isReallyHit = true;
                    }
                    else if (hitPercentageResult <= 0)
                    {
                        isReallyHit = false;
                    }
                    else
                    {
                        var randomValue = Random.Range(0, 10000) % 100;
                        isReallyHit = randomValue < hitPercentageResult;
                    }
                }

                bool isCritical = false;
                if (isCriticalEnabledByDefault && isReallyHit && damage > 0)
                {
                    var criticalThreshold = fromContainer.GetAttributeValue(criticalAttribute);
                    var randomValue = Random.Range(0, 10000) % 100;
                    if (randomValue < criticalThreshold)
                    {
                        damage *= 2;
                        isCritical = true;
                    }
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
                    CommandType = isHealValue ? CommandResult.Heal : CommandResult.TakeDamage,
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
                    continue;
                }

                Debug.Log($"<color=red>{targetUnit.name} is Dead!</color>");

                var dieCommand = new PvDieCommand
                {
                    ResultId = resultId,
                    AbilityId = ability.ID,
                    OwnerId = targetUnit.ID,
                    TargetCellIndex = targetUnit.GetCell().GetIndex(),
                    CommandType = CommandResult.TakeDamage,
                    Value = beforeHealth - afterHealth,
                    TargetUnit = targetUnit
                };

                results.Enqueue(dieCommand);
            }
        }
    }
}
