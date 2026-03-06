using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.Utilities.Runtime.Pools;
using UnityEngine;

namespace IndAssets.Scripts.Commands
{
    public static class PvCommandExtension
    {
        public static void ApplyResultOnVisual(this CommandResult command, PvSoUnitAbility usingAbility,
            IDictionary<string, PvMnBattleGeneralUnit> unitIdCollection)
        {
            if (!unitIdCollection.TryGetValue(command.OwnerId, out var caster))
            {
                return;
            }

            if (command is PvEnergyObtainCommand)
            {
                command.ApplyCommand(null, caster);
                return;
            }

            var targetCell = TacticBattleManager.GetGrid()[command.TargetCellIndex];
            var targetUnit = targetCell.GetUnitOnCell();

            switch (command)
            {
                case PvSimpleDamageCommand:
                case PvPushCommand:
                    if (targetUnit)
                    {
                        targetUnit.LookAtCell(caster.GetCell());
                    }
                    command.ApplyCommand(caster, targetUnit);
                    break;
                case PvStatusApplyCommand:
                    if (targetUnit)
                    {
                        command.ApplyCommand(caster, targetUnit);
                    }
                    break;
                case PvDieCommand:
                    PvDieCommand.GetRaiserUnitDyingEvent.Raise(caster);
                    break;
                case PvGroundStatusCommand:
                    command.ApplyCommand(caster, targetCell);
                    break;
            }

            if (!targetUnit)
            {
                return;
            }

            foreach (var effectPrefab in usingAbility.GetTargetParticles())
            {
                var hitEffect = MnObjectPool.Instance.Get(effectPrefab);
                hitEffect.transform.position = targetUnit.transform.position + Vector3.up * 2;
            }
        }
    }
}