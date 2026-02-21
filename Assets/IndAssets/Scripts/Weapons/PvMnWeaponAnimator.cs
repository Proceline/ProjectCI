using System;
using ProjectCI_Animation.Runtime.Concrete;
using UnityEngine;

namespace IndAssets.Scripts.Weapons
{
    /// <summary>
    /// This weapon Animator normally Only used for Bow and Arrow
    /// </summary>
    public class PvMnWeaponAnimator : MonoBehaviour
    {
        [NonSerialized] private PvMnFunctionalAnimator _animationManager;
        [SerializeField] private Animator animator;
        
        private void Start()
        {
            _animationManager = GetComponentInParent<PvMnFunctionalAnimator>();
            if (_animationManager)
            {
                _animationManager.OnForcePlayAnimation += OnNamedAnimationPlayed;
            }
        }

        private void OnDisable()
        {
            if (_animationManager)
            {
                _animationManager.OnForcePlayAnimation -= OnNamedAnimationPlayed;
            }
        }

        /// <summary>
        /// Originally, only Bow and Arrow requires trigger animation
        /// </summary>
        /// <param name="animName"></param>
        private void OnNamedAnimationPlayed(string animName)
        {
            animator.SetTrigger(animName);
        }
    }
}