using UnityEngine;
using ProjectCI_Animation.Runtime.Interface;
namespace BattleCore.Animation
{
    [CreateAssetMenu(fileName = "PvSoAnimationClipInfo", menuName = "ProjectCI/Animation/PvSoAnimationClipInfo")]
    public class PvSoAnimationClipInfo : ScriptableObject, IAnimationClipInfo
    {
        [SerializeField]
        private AnimationClip _clip;
        [SerializeField]
        private float _transitDuration;
        [SerializeField]
        private float[] _breakPoints;
        
        public AnimationClip Clip => _clip;
        public float TransitDuration => _transitDuration;
        public float[] BreakPoints => _breakPoints;
    }
}
