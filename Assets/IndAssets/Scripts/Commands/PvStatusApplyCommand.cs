using System;
using System.Collections.Generic;
using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    [StaticInjectableTarget]
    public class PvStatusApplyCommand : CommandResult
    {
        [Inject] private static readonly IOnStatusApplyEvent RaiserStatusViewApplyEvent;
        
        public PvSoPassiveStatus StatusType { get; set; }
        private GridPawnUnit TargetUnit { get; set; }

        public override void AddReaction(UnitAbilityCore ability, Queue<Action<GridPawnUnit>> reactions)
        {
            var targetCell = TacticBattleManager.GetGrid()[TargetCellIndex];
            if (!targetCell) return;
            TargetUnit = targetCell.GetUnitOnCell();
            reactions.Enqueue(ApplyVisualEffects);
        }

        private void ApplyVisualEffects(GridPawnUnit owner)
        {
            var targetUnit = TargetUnit;
            if (!targetUnit)
            {
                return;
            }

            RaiserStatusViewApplyEvent.Raise(targetUnit, StatusType);
        }
    }
}