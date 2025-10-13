using System;
using ProjectCI_Animation.Runtime.Concrete;
using UnityEngine;

namespace IndAssets.Scripts.Weapons
{
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

        private void OnNamedAnimationPlayed(string animName)
        {
            animator.SetTrigger(animName);
        }
    }
}