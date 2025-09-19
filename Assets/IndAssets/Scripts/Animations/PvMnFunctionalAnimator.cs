using System;
using System.Collections;
using ProjectCI.CoreSystem.DependencyInjection;
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
        private PvSoSimpleDamageApplyEvent onSimpleDamageApplyEvent;

        [SerializeField] 
        private float generalAdjustOnTransition = 0.15f;
        
        [NonSerialized] private IEventOwner _animatorOwner;
        [NonSerialized] private bool _initialized;

        [NonSerialized] private bool _isLockableAnimating;
        private Coroutine _lockingRoutine;

        public void Initialize<T>(T gridObject) where T : GridObject, IEventOwner
        {
            _animatorOwner = gridObject;
            if (_initialized)
            {
                return;
            }

            onSimpleDamageApplyEvent?.RegisterCallback(RespondToDamageParams);
            _onIsAnimatingProgressFunc.RegisterDelegate(gridObject, IsLockableAnimating);
            _initialized = true;
        }

        private void OnDisable()
        {
            if (_initialized)
            {
                onSimpleDamageApplyEvent?.UnregisterCallback(RespondToDamageParams);
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

            ForcePlayAnimation(AnimationIndexName.Hit);
            var actingTime = GetPresetAnimationDuration(AnimationIndexName.Hit) - generalAdjustOnTransition;
            _lockingRoutine = StartCoroutine(EnablePresetTimeLock(actingTime));
        }

        private IEnumerator EnablePresetTimeLock(float lockTime)
        {
            _isLockableAnimating = true;
            yield return GameUtils.WaitSecondsNoAlloc(lockTime);
            _isLockableAnimating = false;
        }
    }
}