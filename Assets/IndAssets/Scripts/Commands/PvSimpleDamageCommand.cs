using System;
using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public class PvSimpleDamageCommand : CommandResult
    {
        public int BeforeValue;
        public int AfterValue;
        public PvEnDamageType DamageType;

        [NonSerialized]
        private UnitAbilityCore _runtimeAbility;

        public override void AddReaction(UnitAbilityCore ability, List<Action<GridPawnUnit, LevelCellBase>> reactions)
        {
            if (reactions == null)
            {
                return;
            }
            _runtimeAbility = ability;
            reactions.Add(ApplyVisualEffects);
        }

        private void ApplyVisualEffects(GridPawnUnit owner, LevelCellBase target)
        {
            if (!_runtimeAbility)
            {
                return;
            }

            _runtimeAbility.ApplyVisualEffects(owner, target);

            var targetObj = target.GetObjectOnCell();
            if (!targetObj)
            {
                return;
            }

            FeLiteGameRules.XRaiserSimpleDamageApplyEvent.Raise(BeforeValue, AfterValue, Value, owner,
                targetObj, DamageType);
        }
    }
} 