using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    [Serializable]
    public class UnitPureEventParam : IEventParameter
    {
        public PvMnBattleGeneralUnit unit;
    }

    public interface IUnitEmphasisEvent
    {
        void Raise(PvMnBattleGeneralUnit inUnit);
        void RegisterCallback(UnityAction<IEventOwner, UnitPureEventParam> callback);
        void UnregisterCallback(UnityAction<IEventOwner, UnitPureEventParam> callback);
    }

    public interface IUnitPrepareEvent : IUnitEmphasisEvent
    {
        // Empty: For Injection
    }
    
    [CreateAssetMenu(fileName = "UnitState Event", menuName = "ProjectCI Utilities/Events/Unit Emphasis Event")]
    public class PvSoUnitEmphasisEvent : SoUnityEventBase<UnitPureEventParam>, IUnitPrepareEvent
    {
        public void Raise(PvMnBattleGeneralUnit inUnit)
        {
            Raise(inUnit, new UnitPureEventParam { unit = inUnit });
        }
    }
}