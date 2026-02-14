using IndAssets.Scripts.Abilities;
using IndAssets.Scripts.Random;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.Utilities.Runtime.Modifiers;
using System;
using System.Collections.Generic;
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
        private AttributeType criticalAmountAttribute;


        [Header("Dynamic")]
        [SerializeField]
        private bool useDynamicAddon;
        [SerializeField]
        private AttributeType dynamicAmountDependency;

        [SerializeField]
        private PvEnDamageForm damageForm;

        [SerializeField] private int basicAddon;

        [Header("Critical")]
        [SerializeField]
        private bool isCriticalEnabledByDefault;

        [Header("Accuracy")] 
        [SerializeField] 
        private bool isAlwaysHitByDefault;

        [SerializeField]
        private PvSoNotifyDamageBeforeRevEvent raiserNotifyDamageBeforeRev;

        [Inject]
        private static IFinalReceiveDamageModifier _receiveDamageModifier;

        private static readonly ServiceLocator<PvSoRandomSeedCentre> RandomSeedProvider = new();

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

            if (useDynamicAddon)
            {
                var dynamicLimit = (fromContainer.GetAttributeValue(dynamicAmountDependency) + 5) / 2;
                if (dynamicLimit > 0)
                {
                    var limitMin = 5 - dynamicLimit;
                    var limitMax = 5 + dynamicLimit;

                    var randomValue = RandomSeedProvider.Service.GetUnrelatedRandomNumber(limitMin, limitMax);

                    damage += randomValue;
                    if (damage <= 0)
                    {
                        damage = 1;
                    }
                }
                else
                {
                    damage += 5;
                }
            }

            var isReallyHit = isAlwaysHitByDefault || passValue > 0;

            var isCritical = false;
            if (isCriticalEnabledByDefault && passValue == 100)
            {
                var criticalAmountAdjustor = fromContainer.GetAttributeValue(criticalAmountAttribute);
                criticalAmountAdjustor += 100;
                if (criticalAmountAdjustor > 100)
                {
                    criticalAmountAdjustor = 100;
                }
                
                var extraDamage = damage * criticalAmountAdjustor / 100;
                damage += extraDamage;
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
                var adjustedFinalDeltaDmg = raiserNotifyDamageBeforeRev.Raise(finalDeltaDamage, targetUnit, fromUnit, (uint)damageForm);

                if (!damageForm.HasFlag(PvEnDamageForm.Support))
                {
                    toContainer.Health.ModifyValue(-adjustedFinalDeltaDmg);
                }
                else
                {
                    toContainer.Health.ModifyValue(adjustedFinalDeltaDmg);
                }

                finalDeltaDamage = adjustedFinalDeltaDmg;
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

            if (damageForm.HasFlag(PvEnDamageForm.Support))
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
                AbilityId = string.Empty,
                OwnerId = targetUnit.ID,
                TargetCellIndex = targetUnit.GetCell().GetIndex()
            };

            results.Enqueue(dieCommand);

        }

        public static void Execute(GridPawnUnit fromUnit, GridPawnUnit targetUnit, Queue<CommandResult> results, 
            int executeValue, bool isHeal, PvEnDamageType damageType)
        {
            var resultId = Guid.NewGuid().ToString();

            var toContainer = targetUnit.RuntimeAttributes;
            var fromContainer = fromUnit.RuntimeAttributes;

            if (targetUnit.IsDead())
            {
                return;
            }

            var beforeHealth = toContainer.Health.CurrentValue;
            if (!isHeal)
            {
                toContainer.Health.ModifyValue(-executeValue);
            }
            else
            {
                toContainer.Health.ModifyValue(executeValue);
            }
            var afterHealth = toContainer.Health.CurrentValue;

            var savingCommand = new PvSimpleDamageCommand
            {
                ResultId = resultId,
                AbilityId = string.Empty,
                OwnerId = fromUnit.ID,
                TargetCellIndex = targetUnit.GetCell().GetIndex(),
                BeforeValue = beforeHealth,
                AfterValue = afterHealth,
                Value = executeValue,
                DamageType = damageType
            };

            if (isHeal)
            {
                savingCommand.ExtraInfo = UnitAbilityCoreExtensions.HealExtraInfoHint;
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
                AbilityId = string.Empty,
                OwnerId = targetUnit.ID,
                TargetCellIndex = targetUnit.GetCell().GetIndex()
            };

            results.Enqueue(dieCommand);

        }

        public override int MockValue(GridPawnUnit fromUnit, GridPawnUnit targetUnit, uint damageForm)
        {
            var toContainer = targetUnit.RuntimeAttributes;
            var fromContainer = fromUnit.RuntimeAttributes;

            var damage = fromContainer.GetAttributeValue(attackerAttribute) + basicAddon;
            var deltaDamage = Mathf.Max(damage - toContainer.GetAttributeValue(defenderAttribute), 0);

            var finalDeltaDamage = deltaDamage;
            if (targetUnit is IEventOwner damageReceiver)
            {
                finalDeltaDamage = _receiveDamageModifier.CalculateResult(damageReceiver, deltaDamage);
            }

            var adjustedFinalDeltaDmg = raiserNotifyDamageBeforeRev.Raise(finalDeltaDamage, targetUnit, fromUnit, damageForm);

            var translatedDamageForm = (PvEnDamageForm)damageForm;
            var deltaValue = translatedDamageForm.HasFlag(PvEnDamageForm.Support) ? adjustedFinalDeltaDmg : -adjustedFinalDeltaDmg;

            return deltaValue;
        }
    }
}
