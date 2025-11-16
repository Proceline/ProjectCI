using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public class PvRideCommand : PvConcreteCommand
    {
        public int Value;
        
        public override void ApplyCommand(GridPawnUnit fromUnit, GridPawnUnit toUnit)
        {
            if (!toUnit)
            {
                return;
            }

            if (fromUnit.GetTeam() != toUnit.GetTeam())
            {
                throw new Exception("ERROR: Invalid Team for target");
            }

            if (toUnit is not PvMnBattleGeneralUnit targetUnit)
            {
                return;
            }
            
            // ShowEffectOnTarget(TargetCell.transform.position);
            if (Value > 0)
            {
                var cell = fromUnit.GetCell();
                fromUnit.SetCurrentCell(null);
                targetUnit.ForceMoveToCellImmediately(cell);
                
                // TODO: Change Hide process
                fromUnit.gameObject.SetActive(false);
            }
            else if (fromUnit is PvMnBattleGeneralUnit transformingUnit)
            {
                var cell = targetUnit.GetCell();
                targetUnit.SetCurrentCell(null);
                transformingUnit.ForceMoveToCellImmediately(cell);

                if (transformingUnit.GetUnitData() is PvSoBattleUnitData detail)
                {
                    transformingUnit.UpdateAnimationToRide(detail);
                }

                // TODO: Change Hide process
                targetUnit.gameObject.SetActive(false);
            }
        }
    }
} 