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
            var lastRecordedDamage = _recordedDamage.ContainsKey(resultId) ? _recordedDamage[resultId] : 0;
            _recordedDamage[resultId] = lastRecordedDamage + damage;
            _recordedDamageTypes[resultId] = damageType;
        }

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase currentTargetCell, Queue<CommandResult> results, int passValue)
        {
            if (!_recordedDamage.TryGetValue(resultId, out var recordedDamage))
            {
                return;
            }

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
            var adjustedFinalDeltaDmg = raiserNotifyDamageBeforeRev.Raise(finalDeltaDamage, targetUnit, fromUnit, (uint)damageForm);
            finalDeltaDamage = adjustedFinalDeltaDmg;

            if (!_recordedDamageTypes.TryGetValue(resultId, out var damageType))
            {
                damageType = PvEnDamageType.None;
            }

            PvSimpleDamageCommand.AddDamageLikeCommandToHealth(resultId,
                fromUnit, currentTargetCell, toContainer,
                finalDeltaDamage, damageForm, damageType, PvEnDamageReact.ActualHit, results);

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

            _recordedDamage.Remove(resultId);
            _recordedDamageTypes.Remove(resultId);
        }

        public override int MockValue(GridPawnUnit fromUnit, GridPawnUnit targetUnit, uint damageForm) => 0;
    }
}
