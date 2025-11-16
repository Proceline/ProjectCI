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
        public static void ApplyResultOnVisual(this CommandResult commandResult,
            IDictionary<string, PvMnBattleGeneralUnit> unitIdCollection,
            IDictionary<string, PvSoUnitAbility> abilityIdCollection)
        {
            if (!unitIdCollection.TryGetValue(commandResult.OwnerId, out var caster))
            {
                return;
            }

            if (!abilityIdCollection.TryGetValue(commandResult.AbilityId, out var ability))
            {
                return;
            }

            var targetCell = TacticBattleManager.GetGrid()[commandResult.TargetCellIndex];
            var targetUnit = targetCell.GetUnitOnCell();

            switch (commandResult)
            {
                case PvSimpleDamageCommand:
                    if (targetUnit)
                    {
                        targetUnit.LookAtCell(caster.GetCell());
                    }

                    if (commandResult.ExtraInfo != UnitAbilityCoreExtensions.MissExtraInfoHint &&
                        !string.IsNullOrEmpty(commandResult.AbilityId))
                    {
                        foreach (var effectPrefab in ability.GetTargetParticles())
                        {
                            var hitEffect = MnObjectPool.Instance.Get(effectPrefab);
                            hitEffect.transform.position = targetUnit.transform.position + Vector3.up * 2;
                        }
                    }

                    commandResult.ApplyCommand(caster, targetUnit);
                    break;
                case PvPushCommand:
                    commandResult.ApplyCommand(caster, targetCell);
                    break;
                case PvStatusApplyCommand:
                    if (targetUnit)
                    {
                        commandResult.ApplyCommand(caster, targetUnit);
                    }

                    break;
                case PvDieCommand:
                    PvDieCommand.GetRaiserUnitDyingEvent.Raise(caster);
                    break;
            }
        }
    }
}