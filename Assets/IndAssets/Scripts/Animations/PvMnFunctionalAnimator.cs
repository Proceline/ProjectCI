using System;
using System.Collections;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Animation;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;
using ProjectCI.Utilities.Runtime.Events;
using ProjectCI.Utilities.Runtime.Functions;
using UnityEngine;

namespace ProjectCI_Animation.Runtime.Concrete
{
    [StaticInjectableTarget]
    public class PvMnFunctionalAnimator : UnitAnimationManager
    {
        [Inject] private static PvSoOutBooleanFunction _onIsAnimatingProgressFunc;

        [SerializeField] 
        private float generalAdjustOnTransition = 0.15f;
        
        [NonSerialized] private IEventOwner _animatorOwner;
        [NonSerialized] private bool _initialized;

        [NonSerialized] private bool _isLockableAnimating;
        private Coroutine _lockingRoutine;

        [Inject] private static IUnitDyingEvent _onIsDyingEvent;

        public void Initialize<T>(T gridObject) where T : GridObject, IEventOwner
        {
            _animatorOwner = gridObject;
            if (_initialized)
            {
                return;
            }

            FeLiteGameRules.XRaiserSimpleDamageApplyEvent.RegisterCallback(RespondToDamageParams);
            _onIsDyingEvent.RegisterCallback(RespondToDie);
            
            _onIsAnimatingProgressFunc.RegisterDelegate(gridObject, IsLockableAnimating);
            _initialized = true;
        }

        private void OnDisable()
        {
            if (_initialized)
            {
                FeLiteGameRules.XRaiserSimpleDamageApplyEvent.UnregisterCallback(RespondToDamageParams);
                _onIsDyingEvent.UnregisterCallback(RespondToDie);
                
                _initialized = false;
            }
        }

        private bool IsLockableAnimating(IEventOwner checkingOwner)
        {
            return checkingOwner == _animatorOwner && _isLockableAnimating;
        }

        private void RespondToDamageParams(IEventOwner owner, DamageDescriptionParam damageParams)
        {
            if (_animatorOwner.EventIdentifier != damageParams.Victim.ID)
            {
                return;
            }

            if (_isLockableAnimating)
            {
                StopCoroutine(_lockingRoutine);
                _isLockableAnimating = false;
            }

            if (damageParams.ContainsTag(UnitAbilityCoreExtensions.MissExtraInfoHint) ||
                damageParams.ContainsTag(UnitAbilityCoreExtensions.HealExtraInfoHint))
            {
                return;
            }

            var showingValue = damageParams.FinalDamageBeforeAdjusted;
            var animationIndex = showingValue <= 0 ? AnimationIndexName.Defend : AnimationIndexName.Hit;
            ForcePlayAnimation(animationIndex);
            var actingTime = GetPresetAnimationDuration(animationIndex) - generalAdjustOnTransition;
            _lockingRoutine = StartCoroutine(EnablePresetTimeLock(actingTime));
        }
        
        private void RespondToDie(IEventOwner owner, UnitPureEventParam unitParam)
        {
            if (_animatorOwner.EventIdentifier != unitParam.unit.ID)
            {
                return;
            }

            if (_isLockableAnimating)
            {
                StopCoroutine(_lockingRoutine);
                _isLockableAnimating = false;
            }

            PlayLoopAnimation(CurrentPlayableSupport.GetAnimationIndex(AnimationPvCustomName.DieStay.ToString()));
            ForcePlayAnimation(AnimationIndexName.Death);
        }

        private IEnumerator EnablePresetTimeLock(float lockTime)
        {
            _isLockableAnimating = true;
            yield return GameUtils.WaitSecondsNoAlloc(lockTime);
            _isLockableAnimating = false;
        }
    }
}