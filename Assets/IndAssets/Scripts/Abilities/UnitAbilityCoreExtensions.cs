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

        public static bool IsAbilityFollowUpAllowed(this UnitAbilityCore abilityCore)
        {
            if (abilityCore.additionalParameters.Length < 1)
            {
                return true;
            }
            return abilityCore.additionalParameters[0] == 0;
        }

        public static bool IsInitiativeMandatoryFollowUp(this UnitAbilityCore abilityCore)
        {
            return false;
        }

        public static List<CombatActionContext> CreateCombatActionContextList(this UnitAbilityCore abilityCore, bool bIsCounterReachable, FollowUpCondition followUpCondition)
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
                if (bIsCounterReachable)
                {
                    combatActionContextList.Add(new CombatActionContext
                    {
                        IsVictim = true,
                        InitiativeType = InitiativeType.Counter
                    });
                }

                if (followUpCondition == FollowUpCondition.CounterFollowUp && bIsCounterReachable)
                {
                    combatActionContextList.Add(new CombatActionContext
                    {
                        IsVictim = true,
                        InitiativeType = InitiativeType.FollowUp
                    });
                }
                else if (followUpCondition == FollowUpCondition.InitiativeFollowUp)
                {
                    combatActionContextList.Add(new CombatActionContext
                    {
                        IsVictim = false,
                        InitiativeType = InitiativeType.FollowUp
                    });
                }
            }
            return combatActionContextList;
        }
    }
}