using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public interface ITargetUnitEvent
    {
        void Raise(PvMnBattleGeneralUnit inTeam);
        void RegisterCallback(UnityAction<PvMnBattleGeneralUnit> callback);
        void UnregisterCallback(UnityAction<PvMnBattleGeneralUnit> callback);
    }

    public interface ITargetUnitUpdateStatusEvent : ITargetUnitEvent
    {
        
    }
    
    [CreateAssetMenu(fileName = "Status Refresh Event", menuName = "ProjectCI Utilities/Events/Status Refresh Event")]
    public class PvSoTargetUnitEvent : SoUnityEventBase, ITargetUnitUpdateStatusEvent
    {
        private UnityEvent<PvMnBattleGeneralUnit> onRuntimePreInstalledEvents;
        private readonly UnityEvent<PvMnBattleGeneralUnit> _onRuntimePostEvent = new();
        
        public void Raise(PvMnBattleGeneralUnit inTeam)
        {
            onRuntimePreInstalledEvents?.Invoke(inTeam);
            _onRuntimePostEvent?.Invoke(inTeam);
        }

        public void RegisterCallback(UnityAction<PvMnBattleGeneralUnit> callback)
        {
            _onRuntimePostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<PvMnBattleGeneralUnit> callback)
        {
            _onRuntimePostEvent.RemoveListener(callback);
        }
    }
}