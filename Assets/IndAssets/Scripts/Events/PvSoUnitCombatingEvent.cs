using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public enum CombatingQueryType
    {
        None,
        FirstAttempt,
        AutoFollowUp,
        ExtraFollowUp,
        ReplacedFollowUp
    }
    
    public struct CombatingQueryContext
    {
        public bool IsCounter;
        public CombatingQueryType QueryType;
        
        public static bool operator ==(CombatingQueryContext left, CombatingQueryContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CombatingQueryContext left, CombatingQueryContext right)
        {
            return !(left == right);
        }
        
        private bool Equals(CombatingQueryContext other)
        {
            return IsCounter == other.IsCounter && QueryType == other.QueryType;
        }

        public override bool Equals(object obj)
        {
            return obj is CombatingQueryContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsCounter, (int)QueryType);
        }
    }

    public interface IUnitCombatingEvent
    {
        /// <summary>
        /// Raise Event with specific step
        /// </summary>
        /// <param name="inUnit"></param>
        /// <param name="inTarget"></param>
        /// <param name="queryContexts"></param>
        void Raise(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<CombatingQueryContext> queryContexts);
        void RegisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, List<CombatingQueryContext>> callback);
        void UnregisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, List<CombatingQueryContext>> callback);
    }

    public interface IUnitGeneralCombatingEvent : IUnitCombatingEvent
    {
        // Empty
    }

    public interface IUnitCombatingQueryStartEvent : IUnitCombatingEvent
    {
        // Empty
    }

    public interface IUnitCombatingQueryEndEvent : IUnitCombatingEvent
    {
        // Empty
    }

    [CreateAssetMenu(fileName = "Unit Combating Event", menuName = "ProjectCI Utilities/Events/Unit Combating Event")]
    public class PvSoUnitCombatingEvent : SoUnityEventBase, IUnitGeneralCombatingEvent,
        IUnitCombatingQueryStartEvent, IUnitCombatingQueryEndEvent
    {
        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, List<CombatingQueryContext>> onRuntimePreInstalledEvents;

        private readonly UnityEvent<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, List<CombatingQueryContext>> _onRuntimePostEvent = new();
        
        public void Raise(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<CombatingQueryContext> queryContexts)
        {
            onRuntimePreInstalledEvents?.Invoke(inUnit, inTarget, queryContexts);
            _onRuntimePostEvent?.Invoke(inUnit, inTarget, queryContexts);
        }

        public void RegisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, List<CombatingQueryContext>> callback)
        {
            _onRuntimePostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, List<CombatingQueryContext>> callback)
        {
            _onRuntimePostEvent.RemoveListener(callback);
        }
    }
}