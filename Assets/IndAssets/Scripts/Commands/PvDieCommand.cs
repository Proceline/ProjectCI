using ProjectCI.CoreSystem.DependencyInjection;
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
        public static IUnitDyingEvent GetRaiserUnitDyingEvent => _raiserUnitDyingEvent;
    }
} 