using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    [StaticInjectableTarget]
    public class PvStatusApplyCommand : CommandResult
    {
        [Inject] private static readonly IOnStatusApplyEvent RaiserStatusViewApplyEvent;
        
        public PvSoPassiveStatus StatusType { get; set; }
        
        public override void ApplyCommand(GridPawnUnit fromUnit, GridPawnUnit toUnit)
        {
            RaiserStatusViewApplyEvent.Raise(toUnit, StatusType);
        }
    }
}