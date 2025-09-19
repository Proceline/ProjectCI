using System;
using System.Collections;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI_Animation.Runtime.Concrete
{
    public class PvMnFunctionalAnimator : UnitAnimationManager
    {
        [SerializeField]
        private PvSoSimpleDamageApplyEvent onSimpleDamageApplyEvent;
        
        [NonSerialized] private IEventOwner _animatorOwner;
        [NonSerialized] private bool _initialized;

        [NonSerialized] private bool _isLockableAnimating;
        private Coroutine _lockingRoutine;

        public void Initialize(IEventOwner animatorParent)
        {
            _animatorOwner = animatorParent;
            if (_initialized)
            {
                return;
            }

            onSimpleDamageApplyEvent?.RegisterCallback(RespondToDamageParams);
            _initialized = true;
        }

        private void OnDisable()
        {
            if (_initialized)
            {
                onSimpleDamageApplyEvent?.UnregisterCallback(RespondToDamageParams);
            }
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
            var actingTime = GetPresetAnimationDuration(AnimationIndexName.Hit);
            _lockingRoutine = StartCoroutine(EnablePresetTimeLock(actingTime));
        }

        private IEnumerator EnablePresetTimeLock(float lockTime)
        {
            _isLockableAnimating = true;
            yield return Awaitable.WaitForSecondsAsync(lockTime);
            _isLockableAnimating = false;
        }
    }
}