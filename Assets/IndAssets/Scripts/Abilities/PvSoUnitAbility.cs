using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities.Enums;
using ProjectCI.CoreSystem.Runtime.Abilities.Projectiles;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
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

        [SerializeField] 
        private PvMnProjectile projectilePrefab;

        [HideInInspector]
        public UnitAbilityAnimation abilityAnimation;

        public PvMnProjectile ProjectilePrefab => projectilePrefab;
        
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
                new()
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
        
        public override void ApplyVisualEffects(GridPawnUnit inCasterUnit, LevelCellBase inEffectCell)
        {
            // GridObject targetObj = inEffectCell.GetObjectOnCell();
            GridPawnUnit targetExecuteUnit = inEffectCell.GetUnitOnCell();
        
            if (targetExecuteUnit)
            {
                targetExecuteUnit.LookAtCell(inCasterUnit.GetCell());
            }
        
            // TODO: Visual effects on caster, SUCH AS slash light
            // foreach (AbilityParticle abilityParticle in m_SpawnOnCaster)
            // {
            //     Vector3 pos = inCasterUnit.GetCell().GetAllignPos(inCasterUnit);
            //     AbilityParticle createdAbilityParticle = Instantiate(abilityParticle.gameObject, pos, inCasterUnit.transform.rotation).GetComponent<AbilityParticle>();
            //     createdAbilityParticle.Setup(this, inCasterUnit, inEffectCell);
            // }
        
            // TODO: Visual effects on target
            // foreach (AbilityParticle abilityParticle in m_SpawnOnTarget)
            // {
            //     Vector3 pos = inEffectCell.gameObject.transform.position;
            //
            //     if (targetObj)
            //     {
            //         pos = inEffectCell.GetAllignPos(targetObj);
            //     }
            //
            //     AbilityParticle createdAbilityParticle = Instantiate(abilityParticle.gameObject, pos, inEffectCell.transform.rotation).GetComponent<AbilityParticle>();
            //     createdAbilityParticle.Setup(this, inCasterUnit, inEffectCell);
            // }
        
            // TODO: Should be handled as visual effects
            // foreach (StatusEffect ailment in m_Ailments)
            // {
            //     if (ailment)
            //     {
            //         if (targetExecuteUnit)
            //         {
            //             targetExecuteUnit.GetAilmentContainer().AddStatusEffect(inCasterUnit, ailment);
            //         }
            //
            //         CellStatusEffect cellStatusEffect = ailment as CellStatusEffect;
            //         if (cellStatusEffect)
            //         {
            //             inEffectCell.GetAilmentContainer().AddStatusEffect(inCasterUnit, cellStatusEffect, inEffectCell);
            //         }
            //     }
            // }
        }
    }
}