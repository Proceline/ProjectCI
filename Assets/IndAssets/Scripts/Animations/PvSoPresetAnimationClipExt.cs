using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI_Animation.Runtime;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "PvSoPresetAnimationClip", menuName = "ProjectCI Tools/Animations/PvSoPresetClipExt")]
    public class PvSoPresetAnimationClipExt : UnitAbilityAnimation
    {
        [SerializeField] private AnimationPvCustomName customAnimIndexName;
        [NonSerialized] private UnitAnimationManager _lastPlayedAnimationManager;

        public override void PlayAnimation(GridPawnUnit unit)
        {
            var animationManager = unit.GetComponent<UnitAnimationManager>();
            if (!animationManager) return;
            _lastPlayedAnimationManager = animationManager;
            animationManager.ForcePlayAnimation(customAnimIndexName.ToString());
        }

        public override float ExecuteAfterTime(int executeOrder) 
        {
            if (!_lastPlayedAnimationManager) return 0;
            var breakPoints = _lastPlayedAnimationManager
                .GetPresetAnimationBreakPoints(customAnimIndexName.ToString());
            if(breakPoints.Length > executeOrder)
            {
                return breakPoints[executeOrder];
            }
            return 0;
        }

        public override float GetAnimationLength()
        {
            return _lastPlayedAnimationManager ? _lastPlayedAnimationManager.GetPresetAnimationDuration(customAnimIndexName.ToString()) : 1f;
        }
    }
}
