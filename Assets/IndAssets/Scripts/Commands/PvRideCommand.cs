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
        public override void AddReaction(UnitAbilityCore ability, Queue<Action<GridPawnUnit>> reactions)
        {
            base.AddReaction(ability, reactions);
            reactions.Enqueue(DetermineRideSituation);
        }

        private void DetermineRideSituation(GridPawnUnit owner)
        {
            var targetObj = TargetCell.GetObjectOnCell();
            if (!targetObj)
            {
                return;
            }

            if (owner.GetTeam() != targetObj.GetTeam())
            {
                throw new Exception("ERROR: Invalid Team for target");
            }

            if (targetObj is not PvMnBattleGeneralUnit targetUnit)
            {
                return;
            }
            
            ShowEffectOnTarget(TargetCell.transform.position);
            if (Value > 0)
            {
                var cell = owner.GetCell();
                owner.SetCurrentCell(null);
                targetUnit.ForceMoveToCellImmediately(cell);
                
                // TODO: Change Hide process
                owner.gameObject.SetActive(false);
            }
            else if (owner is PvMnBattleGeneralUnit transformingUnit)
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