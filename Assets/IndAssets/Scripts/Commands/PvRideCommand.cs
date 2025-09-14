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
    public class PvRideCommand : CommandResult
    {
        [NonSerialized]
        private UnitAbilityCore _runtimeAbility;

        public override void AddReaction(UnitAbilityCore ability, List<Action<GridPawnUnit, LevelCellBase>> reactions)
        {
            if (reactions == null)
            {
                return;
            }
            _runtimeAbility = ability;
            reactions.Add(DetermineRideSituation);
        }

        private void DetermineRideSituation(GridPawnUnit owner, LevelCellBase target)
        {
            if (!_runtimeAbility)
            {
                return;
            }

            var targetObj = target.GetObjectOnCell();
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