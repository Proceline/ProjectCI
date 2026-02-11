using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Projectiles;
using ProjectCI.CoreSystem.Runtime.Animation;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
        private bool isAppliedOnSelf = false;

        [SerializeField]
        private bool isSupportAbility = false;

        public bool IsSupportAbility => isSupportAbility;

        [SerializeField] private AttributeType dcAttribute = 10;    // Accurate
        [SerializeField] private AttributeType acAttribute = 11;    // Dodge
        public AttributeType DcAttribute => dcAttribute;
        public AttributeType AcAttribute => acAttribute;
        public bool IsAppliedOnSelf => isAppliedOnSelf;

        [SerializeField] 
        private PvMnProjectile projectilePrefab;

        [SerializeField]
        private AnimationPvCustomName abilityAnimationName;

        [Inject] 
        private static readonly ICombatingOnStartEvent XRaiserCombatingOnStarted;

        [Inject]
        private static readonly ICombatingTurnEndEvent OnLogicallyEndedInTurn;
        
        public PvMnProjectile ProjectilePrefab => projectilePrefab;
        public AnimationPvCustomName AnimationName => abilityAnimationName;

        [FormerlySerializedAs("onTriggerUnitEnteredInCombating")] 
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onUnitEnteredInCombating;
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onUnitEnteredInCombatingAsTarget;
        [FormerlySerializedAs("onTriggerUnitLeftFromCombating")] 
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onUnitLeftFromCombating;
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onUnitLeftFromCombatingAsTarget;
        
        public bool IsCounterAllowed()
        {
            return isCounterAllowed;
        }

        public bool IsFollowUpAllowed() => isFollowUpAllowed;

        public bool IsAbilityWeapon()
        {
            return isAbilityWeapon;
        }

        protected virtual void OnCombatedTurnLogicallyEndedResponse(PvMnBattleGeneralUnit caster,
            PvMnBattleGeneralUnit victim)
        {
            onUnitEnteredInCombatingAsTarget.Invoke(victim);
            
            victim.CounterAbility.onUnitLeftFromCombating.Invoke(caster);
            OnLogicallyEndedInTurn.UnregisterCallback(OnCombatedTurnLogicallyEndedResponse);
        }

        public override void ApplyVisualEffects(GridPawnUnit inCasterUnit, LevelCellBase inEffectCell)
        {
            // GridObject targetObj = inEffectCell.GetObjectOnCell();
            // TODO: Remove Debug.LogError after testing
            Debug.LogError($"Applying visual effect of {name}");
            GridPawnUnit targetExecuteUnit = inEffectCell.GetUnitOnCell();
        
            if (targetExecuteUnit)
            {
                targetExecuteUnit.LookAtCell(inCasterUnit.GetCell());
            }
        }
    }
}