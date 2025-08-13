using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities.Enums;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "NewAbility", menuName = "ProjectCI Tools/Ability/Create Custom Ability", order = 1)]
    public class PvSoUnitAbility : UnitAbilityCore
    {
        [SerializeField]
        private bool isCounterAllowed = true;

        [SerializeField]
        private bool isFollowUpAllowed = true;

        [SerializeField]
        private bool isAbilityWeapon = true;

        [HideInInspector]
        public UnitAbilityAnimation abilityAnimation;

        public bool IsCounterAllowed()
        {
            return isCounterAllowed;
        }

        public bool IsFollowUpAllowed()
        {
            return isFollowUpAllowed;
        }

        public bool IsAbilityWeapon()
        {
            return isAbilityWeapon;
        }

        public List<CombatActionContext> CreateCombatActionContextList(bool bIsCounterReachable, FollowUpCondition followUpCondition)
        {
            List<CombatActionContext> combatActionContextList = new List<CombatActionContext>()
            {
                new CombatActionContext
                {
                    IsVictim = false,
                    InitiativeType = InitiativeType.Initiative
                }
            };
            if (IsCounterAllowed())
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