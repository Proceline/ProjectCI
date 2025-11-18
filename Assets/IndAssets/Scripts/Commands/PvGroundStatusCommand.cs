using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    public class PvGroundStatusCommand : CommandResult
    {
        public PvSoGroundStatus RelatedGroundStatus { get; set; }

        public override void ApplyCommand(GridPawnUnit fromUnit, LevelCellBase targetCell)
        {
            RelatedGroundStatus.RefreshVisualGroundStatus(targetCell);
        }
    }
}