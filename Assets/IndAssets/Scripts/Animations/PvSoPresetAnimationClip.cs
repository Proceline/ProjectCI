using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI_Animation.Runtime;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "PvSoPresetAnimationClip", menuName = "ProjectCI Tools/Animations/PvSoPresetClip")]
    public class PvSoPresetAnimationClip : UnitAbilityAnimation
    {
        [SerializeField] private AnimationIndexName m_AnimationIndexName;
        [NonSerialized] private UnitAnimationManager _lastPlayedAnimationManager;

        public override void PlayAnimation(GridPawnUnit InUnit)
        {
            var animationManager = InUnit.GetComponent<UnitAnimationManager>();
            if(animationManager)
            {
                _lastPlayedAnimationManager = animationManager;
                animationManager.ForcePlayAnimation(m_AnimationIndexName);
            }
        }

        public override float ExecuteAfterTime(int executeOrder) 
        {
            if(_lastPlayedAnimationManager)
            {
                var breakPoints = _lastPlayedAnimationManager
                    .GetPresetAnimationBreakPoints(m_AnimationIndexName);
                if(breakPoints.Length > executeOrder)
                {
                    return breakPoints[executeOrder];
                }
            }
            return 0;
        }

        public override float GetAnimationLength()
        {
            if(_lastPlayedAnimationManager)
            {
                return _lastPlayedAnimationManager.GetPresetAnimationDuration(m_AnimationIndexName);
            }
            return 1f;
        }
    }
}
