using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.Utilities.Runtime.Events;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoDynamicDamageAbilityParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoDynamicDamageAbilityParams")]
    public class PvSoDynamicDamageAbilityParams : AbilityParamBase
    {
        private readonly Dictionary<string, int> _recordedDamage = new();
        private readonly Dictionary<string, PvEnDamageType> _recordedDamageTypes = new();

        [SerializeField]
        private int basicAddon;

        [SerializeField]
        private PvEnDamageForm damageForm;

        [SerializeField]
        private PvSoNotifyDamageBeforeRevEvent raiserNotifyDamageBeforeRev;

        public void SetupDynamicDamage(string resultId, int damage, PvEnDamageType damageType)
        {
            Debug.LogError("setup dynamic damage: " + damage + " " + resultId);
            var lastRecordedDamage = _recordedDamage.ContainsKey(resultId) ? _recordedDamage[resultId] : 0;
            _recordedDamage[resultId] = lastRecordedDamage + damage;
            _recordedDamageTypes[resultId] = damageType;
        }

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase currentTargetCell, Queue<CommandResult> results, int passValue)
        {
            Debug.LogError(resultId);
            if (!_recordedDamage.TryGetValue(resultId, out var recordedDamage))
            {
                return;
            }

            Debug.LogError("Successfully get into");

            var finalDeltaDamage = recordedDamage + basicAddon;
            if (finalDeltaDamage == 0)
            {
                return;
            }

            var targetUnit = currentTargetCell.GetUnitOnCell();

            if (!targetUnit || targetUnit.IsDead())
            {
                return;
            }

            var toContainer = targetUnit.RuntimeAttributes;
            var fromContainer = fromUnit.RuntimeAttributes;
            var beforeHealth = toContainer.Health.CurrentValue;

            var adjustedFinalDeltaDmg = raiserNotifyDamageBeforeRev.Raise(finalDeltaDamage, resultId,
                targetUnit, fromUnit, (uint)damageForm);

            if (!damageForm.HasFlag(PvEnDamageForm.Support))
            {
                toContainer.Health.ModifyValue(-adjustedFinalDeltaDmg);
            }
            else
            {
                toContainer.Health.ModifyValue(adjustedFinalDeltaDmg);
            }

            finalDeltaDamage = adjustedFinalDeltaDmg;

            var afterHealth = toContainer.Health.CurrentValue;

            if (!_recordedDamageTypes.TryGetValue(resultId, out var damageType))
            {
                damageType = PvEnDamageType.None;
            }

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

            _recordedDamage.Remove(resultId);
            _recordedDamageTypes.Remove(resultId);
        }

        public override int MockValue(GridPawnUnit fromUnit, GridPawnUnit targetUnit, uint damageForm) => 0;
    }
}
