using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI_Animation.Runtime;
using ProjectCI_Animation.Runtime.Interface;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "PvSoFunctionalAnimationClip", menuName = "ProjectCI Tools/Animations/PvSoFunctionalClip")]
    public class PvSoFunctionalAnimationClip : UnitAbilityAnimation, IAnimationClipInfo
    {
        [SerializeField]
        private AnimationClip m_AnimationClip;

        [SerializeField]
        private float m_TransitDuration;

        [SerializeField]
        private float[] m_BreakPoints;

        public AnimationClip Clip => m_AnimationClip;
        public float TransitDuration => m_TransitDuration;
        public float[] BreakPoints => m_BreakPoints;

        public override void PlayAnimation(GridPawnUnit InUnit)
        {
            var animationManager = InUnit.GetComponent<UnitAnimationManager>();
            if(animationManager)
            {
                animationManager.ForcePlayAnimation(this);
            }
        }

        public override float ExecuteAfterTime(int executeOrder) => m_BreakPoints[executeOrder];
        public override float GetAnimationLength() => m_AnimationClip.length;
    }
}
