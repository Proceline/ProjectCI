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

        [SerializeField]
        private PvEnDamageForm damageForm;

        [SerializeField] private int basicAddon;

        [SerializeField]
        private bool isReactionRequired = true;

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

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase currentTargetCell, Queue<CommandResult> results, int passValue, params uint[] damageInfos)
        {
            var targetUnit = currentTargetCell.GetUnitOnCell();
            if (!targetUnit || !targetUnit.gameObject.activeSelf)
            {
                return;
            }

            var toContainer = targetUnit.RuntimeAttributes;
            var fromContainer = fromUnit.RuntimeAttributes;

            if (targetUnit.IsDead())
            {
                return;
            }

            var combinedDamageForm = (uint)damageForm | damageInfos[0];
            var damage = fromContainer.GetAttributeValue(attackerAttribute) + basicAddon;

            var isReallyHit = isAlwaysHitByDefault || passValue > 0;

            var isCritical = false;
            if (isCriticalEnabledByDefault && passValue == 100)
            {
                var suggestedCritAmount = fromContainer.GetAttributeValue(criticalAmountAttribute);
                if (suggestedCritAmount > damage)
                {
                    damage += suggestedCritAmount;
                }
                else
                {
                    damage *= 2;
                }

                isCritical = true;
            }

            var deltaDamage =
                isReallyHit ? Mathf.Max(damage - toContainer.GetAttributeValue(defenderAttribute), 0) : 0;

            var finalDeltaDamage = deltaDamage;
            if (targetUnit is IEventOwner damageReceiver)
            {
                finalDeltaDamage = _receiveDamageModifier.CalculateResult(damageReceiver, deltaDamage);
            }

            var victimReaction = PvEnDamageReact.MissHit;
            if (isReallyHit)
            {
                victimReaction |= isReactionRequired ? PvEnDamageReact.ActualHit : PvEnDamageReact.IgnoreHit;
                if (isCritical)
                {
                    victimReaction |= PvEnDamageReact.Critical;
                }

                var adjustedFinalDeltaDmg = raiserNotifyDamageBeforeRev.Raise(finalDeltaDamage, targetUnit, fromUnit, combinedDamageForm);
                finalDeltaDamage = adjustedFinalDeltaDmg;
            }

            PvSimpleDamageCommand.AddDamageLikeCommandToHealth(resultId, 
                fromUnit, currentTargetCell, toContainer, 
                finalDeltaDamage, (PvEnDamageForm)combinedDamageForm, damageType, victimReaction, results);

            PvEnergyObtainCommand.AdjustAndEnqueueEnergy(resultId, fromUnit.ID, fromContainer, 20, results);

            // Add Die Command if Health is 0
            if (!targetUnit.IsDead())
            {
                return;
            }

            Debug.Log($"<color=red>{targetUnit.name} is Dead!</color>");

            var dieCommand = new PvDieCommand
            {
                ResultId = resultId,
                OwnerId = targetUnit.ID,
                TargetCellIndex = targetUnit.GetCell().GetIndex()
            };

            results.Enqueue(dieCommand);

        }

        public override int MockValue(GridPawnUnit fromUnit, GridPawnUnit targetUnit, uint extraDamageForm)
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

            var combinedDamageForm = (PvEnDamageForm)extraDamageForm | damageForm;

            var adjustedFinalDeltaDmg = raiserNotifyDamageBeforeRev.Raise(finalDeltaDamage, targetUnit, fromUnit, (uint)combinedDamageForm);
            var deltaValue = combinedDamageForm.HasFlag(PvEnDamageForm.Support) ? adjustedFinalDeltaDmg : -adjustedFinalDeltaDmg;

            return deltaValue;
        }
    }
}
