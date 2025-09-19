using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI_Animation.Runtime;
using ProjectCI_Animation.Runtime.Interface;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectCI.CoreSystem.Runtime.Animation
{
    [CreateAssetMenu(fileName = "PvSoFunctionalAnimationClip", menuName = "ProjectCI Tools/Animations/PvSoFunctionalClip")]
    public class PvSoFunctionalAnimationClip : UnitAbilityAnimation, IAnimationClipInfo
    {
        [FormerlySerializedAs("m_AnimationClip")] [SerializeField]
        private AnimationClip animationClip;

        [FormerlySerializedAs("m_TransitDuration")] [SerializeField]
        private float transitDuration;

        [FormerlySerializedAs("m_BreakPoints")] [SerializeField]
        private float[] breakPoints;

        public AnimationClip Clip => animationClip;
        public float TransitDuration => transitDuration;
        public float[] BreakPoints => breakPoints;

        public override void PlayAnimation(GridPawnUnit inUnit)
        {
            // Empty
            throw new Exception("<PvSoFunctionalAnimationClip>This is Information-only Scriptable Animation!");
        }

        public override float ExecuteAfterTime(int executeOrder) => breakPoints[executeOrder];
        public override float GetAnimationLength() => animationClip.length;
    }
}
