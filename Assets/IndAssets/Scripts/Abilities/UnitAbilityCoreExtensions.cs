using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities.Enums;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Extensions
{
    public static class UnitAbilityCoreExtensions
    {
        public static bool IsAbilityCounterAllowed(this UnitAbilityCore abilityCore)
        {
            return abilityCore.DoesAllowBlocked();
        }

        public static bool IsInitiativeMandatoryFollowUp(this UnitAbilityCore abilityCore)
        {
            return false;
        }

        public static List<CombatActionContext> CreateCombatActionContextList(this UnitAbilityCore abilityCore, FollowUpCondition followUpCondition)
        {
            List<CombatActionContext> combatActionContextList = new List<CombatActionContext>()
            {
                new CombatActionContext
                {
                    IsVictim = false,
                    InitiativeType = InitiativeType.Initiative
                }
            };
            if (abilityCore.IsAbilityCounterAllowed())
            {
                combatActionContextList.Add(new CombatActionContext
                {
                    IsVictim = true,
                    InitiativeType = InitiativeType.Counter
                });
                
                if (followUpCondition != FollowUpCondition.None)
                {
                    combatActionContextList.Add(new CombatActionContext
                    {
                        IsVictim = followUpCondition == FollowUpCondition.CounterFollowUp,
                        InitiativeType = InitiativeType.FollowUp
                    });
                }
            }
            return combatActionContextList;
        }
    }
}