using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public class PvPushCommand : PvConcreteCommand
    {
        public LevelCellBase FromCell { get; set; }

        public override void AddReaction(UnitAbilityCore ability, Queue<Action<GridPawnUnit>> reactions)
        {
            base.AddReaction(ability, reactions);
            reactions.Enqueue(ApplyVisualEffects);
        }

        private void ApplyVisualEffects(GridPawnUnit owner)
        {
            var fromCell = FromCell;
            var toCell = TargetCell;
            var victim = toCell.GetUnitOnCell();
            if (!victim)
            {
                return;
            }

            var direction = fromCell.GetDirectionToAdjacentCell(toCell);
            for (var i = 0; i < Value; i++)
            {
                var dirCell = toCell.GetAdjacentCell(direction);
                if (dirCell && dirCell.IsCellAccesible())
                {
                    toCell = dirCell;
                }
            }

            victim.ForceMoveTo(toCell);
        }
    }
} 