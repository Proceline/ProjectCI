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
        [NonSerialized] private GridPawnUnit _lastUsingPawn;

        public override void PlayAnimation(GridPawnUnit unit)
        {
            _lastUsingPawn = unit;
            unit.BroadcastActionTriggerByTag(customAnimIndexName.ToString());
        }

        public override float ExecuteAfterTime(int executeOrder) 
        {
            if (!_lastUsingPawn) return 0;
            return _lastUsingPawn.GrabActionValueDataByIndexTag(executeOrder, customAnimIndexName.ToString());
        }

        public override float GetAnimationLength()
        {
            if (!_lastUsingPawn) return 0;
            return _lastUsingPawn.GrabActionValueDataByIndexTag(0, customAnimIndexName.ToString(), "Length");
        }
    }
}
