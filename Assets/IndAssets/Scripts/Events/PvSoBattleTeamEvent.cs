using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ProjectCI.Utilities.Runtime.Events
{
    public interface ITeamRelatedEvent
    {
        void Raise(BattleTeam inTeam);
        void RegisterCallback(UnityAction<BattleTeam> callback);
        void UnregisterCallback(UnityAction<BattleTeam> callback);
    }

    public interface ITeamRoundEndEvent : ITeamRelatedEvent
    {
        
    }
    
    [CreateAssetMenu(fileName = "Battle Team Event", menuName = "ProjectCI Utilities/Events/Team Event")]
    public class PvSoBattleTeamEvent : SoUnityEventBase, ITeamRoundEndEvent
    {
        [FormerlySerializedAs("onRuntimePostExtraEvents"), SerializeField]
        private UnityEvent<BattleTeam> onRuntimePreInstalledEvents;

        private readonly UnityEvent<BattleTeam> _onRuntimePostEvent = new();

        [NonSerialized]
        private bool _bIsPostHandleEventsLaunched;
        
        public void Raise(BattleTeam inTeam)
        {
            if (!_bIsPostHandleEventsLaunched)
            {
                RegisterCallback(onRuntimePreInstalledEvents.Invoke);
                _bIsPostHandleEventsLaunched = true;
            }
            onRuntimePreInstalledEvents?.Invoke(inTeam);
            _onRuntimePostEvent?.Invoke(inTeam);
        }

        public void RegisterCallback(UnityAction<BattleTeam> callback)
        {
            _onRuntimePostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<BattleTeam> callback)
        {
            _onRuntimePostEvent.RemoveListener(callback);
        }
    }
}