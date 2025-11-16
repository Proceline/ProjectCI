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
        public int Distance;
        public LevelCellBase FromCell { get; set; }

        public override void ApplyCommand(GridPawnUnit fromUnit, LevelCellBase targetCell)
        {
            var fromCell = FromCell;
            var victim = targetCell.GetUnitOnCell();
            if (!victim)
            {
                return;
            }

            var direction = fromCell.GetDirectionToAdjacentCell(targetCell);
            for (var i = 0; i < Distance; i++)
            {
                var dirCell = targetCell.GetAdjacentCell(direction);
                if (dirCell && dirCell.IsCellAccessible())
                {
                    targetCell = dirCell;
                }
            }

            victim.ForceMoveTo(targetCell);
        }
    }
} 