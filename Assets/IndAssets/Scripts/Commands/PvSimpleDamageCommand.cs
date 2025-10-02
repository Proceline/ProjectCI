using System;
using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public class PvSimpleDamageCommand : PvConcreteCommand
    {
        public int BeforeValue;
        public int AfterValue;
        public PvEnDamageType DamageType;

        public override void AddReaction(UnitAbilityCore ability, Queue<Action<GridPawnUnit>> reactions)
        {
            base.AddReaction(ability, reactions);
            reactions.Enqueue(ApplyVisualEffects);
        }

        private void ApplyVisualEffects(GridPawnUnit owner)
        {
            RuntimeAbility.ApplyVisualEffects(owner, TargetCell);

            var targetObj = TargetCell.GetObjectOnCell();
            if (!targetObj)
            {
                return;
            }

            if (ExtraInfo != UnitAbilityCoreExtensions.MissExtraInfoHint)
            {
                ShowEffectOnTarget(targetObj.transform.position);
            }

            if (string.IsNullOrEmpty(ExtraInfo))
            {
                FeLiteGameRules.XRaiserSimpleDamageApplyEvent.Raise(BeforeValue, AfterValue, Value, owner,
                    targetObj, DamageType);
            }
            else
            {
                FeLiteGameRules.XRaiserSimpleDamageApplyEvent.Raise(BeforeValue, AfterValue, Value, owner,
                    targetObj, DamageType, ExtraInfo);
            }
        }
    }
} 