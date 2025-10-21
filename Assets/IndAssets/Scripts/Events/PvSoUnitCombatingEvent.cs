using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    [Serializable]
    public class UnitCombatingEventParam : IEventParameter
    {
        public PvMnBattleGeneralUnit unit;
        public PvMnBattleGeneralUnit target;
        public List<CombatingQueryContext> CombatingList { get; internal set; }
    }

    public enum CombatingQueryType
    {
        FirstAttempt,
        AutoFollowUp,
        ExtraFollowUp
    }
    
    public struct CombatingQueryContext
    {
        public bool IsCounter;
        public CombatingQueryType QueryType;
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
        void RegisterCallback(UnityAction<IEventOwner, UnitCombatingEventParam> callback);
        void UnregisterCallback(UnityAction<IEventOwner, UnitCombatingEventParam> callback);
    }

    public interface IUnitGeneralCombatingEvent : IUnitCombatingEvent
    {
        // Empty
    }

    [CreateAssetMenu(fileName = "Unit Combating Event", menuName = "ProjectCI Utilities/Events/Unit Combating Event")]
    public class PvSoUnitCombatingEvent : SoUnityEventBase<UnitCombatingEventParam>, IUnitGeneralCombatingEvent
    {
        [NonSerialized] private UnitCombatingEventParam _bufferedParam;
        [NonSerialized] private bool _hasEverBuffered;

        public void Raise(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<CombatingQueryContext> queryContexts)
        {
            if (!_hasEverBuffered)
            {
                _bufferedParam = new UnitCombatingEventParam
                {
                    unit = inUnit, target = inTarget, CombatingList = queryContexts
                };
                _hasEverBuffered = true;
            }
            else
            {
                _bufferedParam.unit = inUnit;
                _bufferedParam.target = inTarget;
                _bufferedParam.CombatingList = queryContexts;
            }

            Raise(inUnit, _bufferedParam);
            
            // Unlink the CombatingList to boost GC
            _bufferedParam.CombatingList = null;
        }
    }
}