using System;
using ProjectCI_Animation.Runtime;
using ProjectCI_Animation.Runtime.Editor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEditor;

namespace ProjectCI.CoreSystem.Editor.Animations
{
    [CustomEditor(typeof(PvSoAnimationSupportAsset))]
    public class PvSoAnimationSupportAssetEditor : AnimationPlayableSupportBaseEditor
    {
        private string[] _preloadedExtraIndexNames;
        
        protected override string GetNameByIndexInEditor(int index)
        {
            _preloadedExtraIndexNames ??= Enum.GetNames(typeof(AnimationPvCustomName));
            int realIndex = index - (int)AnimationIndexName.EndMarkDontUse;
            if (_preloadedExtraIndexNames.Length > realIndex && realIndex >= 0)
            {
                return _preloadedExtraIndexNames[realIndex];
            }
            return base.GetNameByIndexInEditor(index);
        }
    }
}
