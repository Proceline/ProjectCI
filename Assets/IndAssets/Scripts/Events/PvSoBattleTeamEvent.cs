using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public struct BattleTeamParam : IEventParameter
    {
        public BattleTeam Team;

        public BattleTeamParam(BattleTeam inTeam)
        {
            Team = inTeam;
        }
    }
    
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
    public class PvSoBattleTeamEvent : SoUnityEventBase<BattleTeamParam>, ITeamRoundEndEvent
    {
        private readonly Dictionary<UnityAction<BattleTeam>, UnityAction<IEventOwner, BattleTeamParam>> _converters = new();
        
        [SerializeField]
        private UnityEvent<BattleTeam> onRuntimePostExtraEvents;

        [NonSerialized]
        private bool _bIsPostHandleEventsLaunched;
        
        public void Raise(BattleTeam inTeam)
        {
            if (!_bIsPostHandleEventsLaunched)
            {
                RegisterCallback(onRuntimePostExtraEvents.Invoke);
                _bIsPostHandleEventsLaunched = true;
            }
            Raise(null, new BattleTeamParam(inTeam));
        }

        public void RegisterCallback(UnityAction<BattleTeam> callback)
        {
            if (!_converters.TryGetValue(callback, out var convertedCallback))
            {
                convertedCallback = (_, param) => callback.Invoke(param.Team);
                _converters.Add(callback, convertedCallback);
            }
            RegisterCallback(convertedCallback);
        }

        public void UnregisterCallback(UnityAction<BattleTeam> callback)
        {
            if (_converters.TryGetValue(callback, out var convertedCallback))
            {
                UnregisterCallback(convertedCallback);
            }
        }
    }
}