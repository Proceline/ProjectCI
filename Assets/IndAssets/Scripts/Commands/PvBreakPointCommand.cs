using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.Commands.Concrete
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public class PvBreakPointCommand : CommandResult
    {
        public override void ApplyCommand(GridPawnUnit fromUnit, GridPawnUnit toUnit)
        {
            // Empty
        }

        public override void ClearCommand()
        {
            // Empty
        }
    }
} 