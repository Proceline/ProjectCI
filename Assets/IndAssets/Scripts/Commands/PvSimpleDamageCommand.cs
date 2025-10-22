using System;
using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
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
            var targetObject = TargetObject;
            RuntimeAbility.ApplyVisualEffects(owner, TargetObject.GetCell());
            if (!targetObject)
            {
                return;
            }

            if (ExtraInfo != UnitAbilityCoreExtensions.MissExtraInfoHint)
            {
                ShowEffectOnTarget(targetObject.transform.position);
            }

            if (string.IsNullOrEmpty(ExtraInfo))
            {
                FeLiteGameRules.XRaiserSimpleDamageApplyEvent.Raise(BeforeValue, AfterValue, Value, owner,
                    targetObject, DamageType);
            }
            else
            {
                FeLiteGameRules.XRaiserSimpleDamageApplyEvent.Raise(BeforeValue, AfterValue, Value, owner,
                    targetObject, DamageType, ExtraInfo);
            }
        }
    }
} 