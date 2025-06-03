using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.Units.Extensions
{
    public static class GridPawnUnitExtensions
    {
        public static bool DetermineInitiativeInCombat(this GridPawnUnit unit, GridPawnUnit target)
        {
            return true;
        }

        public static bool IsMandatoryFollowUp(this GridPawnUnit unit, GridPawnUnit target)
        {
            return false;
        }
    }
}