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
    [CreateAssetMenu(fileName = "PvSoBreakPointParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoBreakPointParams")]
    public class PvSoBreakPointParams : AbilityParamBase
    {
        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase currentTargetCell, Queue<CommandResult> results, int passValue)
        {
            var targetUnit = currentTargetCell.GetUnitOnCell();
            if (!targetUnit.IsDead())
            {
                return;
            }

            var savingCommand = new PvSimpleDamageCommand
            {
                //ResultId = resultId,
                //OwnerId = fromUnit.ID,
                //TargetCellIndex = targetUnit.GetCell().GetIndex(),
                //BeforeValue = beforeHealth,
                //AfterValue = afterHealth,
                //Value = finalDeltaDamage,
                //DamageType = damageType
            };

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
                OwnerId = targetUnit.ID,
                TargetCellIndex = targetUnit.GetCell().GetIndex()
            };

            results.Enqueue(dieCommand);

        }

        public override int MockValue(GridPawnUnit fromUnit, GridPawnUnit targetUnit, uint extraDamageForm) => 0;
    }
}
