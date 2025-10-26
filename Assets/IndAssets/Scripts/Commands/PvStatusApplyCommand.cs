using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    public class PvStatusApplyCommand : PvConcreteCommand
    {
        public string StatusType { get; set; }

        public override void AddReaction(UnitAbilityCore ability, Queue<Action<GridPawnUnit>> reactions)
        {
            base.AddReaction(ability, reactions);
            reactions.Enqueue(ApplyVisualEffects);
        }

        private void ApplyVisualEffects(GridPawnUnit owner)
        {
            var targetObject = TargetObject;
            if (!targetObject)
            {
                return;
            }
            // Empty
        }
    }
}