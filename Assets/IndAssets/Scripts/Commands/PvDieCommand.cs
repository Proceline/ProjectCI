using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    [StaticInjectableTarget]
    public class PvDieCommand : CommandResult
    {
        [Inject] private static IUnitDyingEvent _raiserUnitDyingEvent;
        public GridPawnUnit TargetUnit { get; set; }
        
        public override void AddReaction(UnitAbilityCore ability, Queue<Action<GridPawnUnit>> reactions)
        {
            reactions.Enqueue(_ =>
            {
                if (TargetUnit is PvMnBattleGeneralUnit battleUnit)
                {
                    _raiserUnitDyingEvent.Raise(battleUnit);
                }
            });
        }
    }
} 