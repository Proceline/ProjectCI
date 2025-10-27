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
        
        public void Raise(BattleTeam inTeam)
        {
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