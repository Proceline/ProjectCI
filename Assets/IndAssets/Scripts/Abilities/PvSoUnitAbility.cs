using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Projectiles;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "NewAbility", menuName = "ProjectCI Tools/Ability/Create Custom Ability", order = 1)]
    public class PvSoUnitAbility : UnitAbilityCore
    {
        [SerializeField]
        private bool isCounterAllowed = true;

        [SerializeField]
        private bool isFollowUpAllowed = true;

        [SerializeField]
        private bool isAutoFollowUpAllowed = true;

        [SerializeField]
        private bool isAbilityWeapon = true;

        [SerializeField] 
        private PvMnProjectile projectilePrefab;

        [HideInInspector]
        public UnitAbilityAnimation abilityAnimation;

        [Inject] 
        private static readonly ICombatingOnStartEvent XRaiserCombatingOnStarted;
        
        public PvMnProjectile ProjectilePrefab => projectilePrefab;
        
        public bool IsCounterAllowed()
        {
            return isCounterAllowed;
        }

        public bool IsFollowUpAllowed() => isFollowUpAllowed;

        public bool IsAbilityWeapon()
        {
            return isAbilityWeapon;
        }

        public virtual List<CombatingQueryContext> OnCombatingQueryListCreated(PvMnBattleGeneralUnit caster,
            PvMnBattleGeneralUnit victim, bool casterSpeedExceed, bool victimSpeedExceed)
        {
            // You can register on Combating status before this calculation
            XRaiserCombatingOnStarted.Raise(caster, victim);
            
            var combatContextList = new List<CombatingQueryContext>
            {
                new() { IsCounter = false, QueryType = CombatingQueryType.FirstAttempt }
            };

            // Normally, only support abilities don't allow counter
            if (!IsCounterAllowed())
            {
                return combatContextList;
            }

            var targetAbility = victim.EquippedAbility;
            List<LevelCellBase> targetAbilityCells = targetAbility.GetAbilityCells(victim);
            var bIsTargetAbilityAbleToCounter =
                targetAbilityCells.Count > 0 && targetAbilityCells.Contains(caster.GetCell());

            if (bIsTargetAbilityAbleToCounter)
            {
                combatContextList.Add(new CombatingQueryContext
                    { IsCounter = true, QueryType = CombatingQueryType.FirstAttempt });
            }

            if (casterSpeedExceed && isAutoFollowUpAllowed)
            {
                combatContextList.Add(new CombatingQueryContext
                    { IsCounter = false, QueryType = CombatingQueryType.AutoFollowUp });
            }
            else if (bIsTargetAbilityAbleToCounter && victimSpeedExceed && targetAbility.isAutoFollowUpAllowed)
            {
                combatContextList.Add(new CombatingQueryContext
                    { IsCounter = true, QueryType = CombatingQueryType.AutoFollowUp });
            }

            return combatContextList;
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