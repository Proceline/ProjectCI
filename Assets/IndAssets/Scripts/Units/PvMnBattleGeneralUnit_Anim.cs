using System;
using ProjectCI_Animation.Runtime;
using ProjectCI.CoreSystem.Runtime.Animation.Services;
using ProjectCI.CoreSystem.Runtime.Services;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class PvMnBattleGeneralUnit
    {
        private readonly ServiceLocator<PvSoAnimSetsCollection> _animSetService = new();

        public void UpdateAnimationToRide(AnimationPlayableSupportBase animationSupport)
        {
            if (!_animationManager)
            {
                throw new NullReferenceException("ERROR: Animation Essential Component missing!");
            }

            var animSetManager = _animSetService.Service;
            _animationManager.SetupAnimationGraphDetails(animSetManager.defaultRiderAnimation, false);
        }
    }
} 