using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Extensions
{
    public static class UnitAbilityCoreExtensions
    {
        public static bool IsAbilityCounterAllowed(this UnitAbilityCore abilityCore)
        {
            return abilityCore.DoesAllowBlocked();
        }

        public static bool IsAbilityFollowUpAllowed(this UnitAbilityCore abilityCore)
        {
            return abilityCore.GetActionPointCost() > 0;
        }
    }
}