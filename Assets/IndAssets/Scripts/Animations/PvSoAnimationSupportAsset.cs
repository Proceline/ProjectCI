using System;
using System.Collections.Generic;
using ProjectCI_Animation.Runtime;
using ProjectCI_Animation.Runtime.Interface;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Animation
{
    public enum AnimationPvCustomName
    {
        AttackBasic
    }
    
    public class PvCustomAnimationNamesList : IAnimationIndexAddon
    {
        public int GetOriginalIndexByName(string animName)
            => _markedIndicesByNames.GetValueOrDefault(animName, -1);

        private readonly Dictionary<string, int> _markedIndicesByNames = new();
        public string[] AdditionalIndexNames => _preloadNames ??= Enum.GetNames(typeof(AnimationPvCustomName));
        private string[] _preloadNames;

        internal PvCustomAnimationNamesList()
        {
            for (int i = 0; i < AdditionalIndexNames.Length; i++)
            {
                _markedIndicesByNames.Add(AdditionalIndexNames[i], i);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "PvSoAnimationSupportAsset", menuName = "ProjectCI Tools/Animations/PvSoAnimationSupportAsset")]
    public class PvSoAnimationSupportAsset : AnimationPlayableSupportBase<PvSoFunctionalAnimationClip>
    {
        private readonly PvCustomAnimationNamesList _customAnimationNamesList = new();
        protected override IAnimationIndexAddon AnimationIndexAddon => _customAnimationNamesList;
    }
}
