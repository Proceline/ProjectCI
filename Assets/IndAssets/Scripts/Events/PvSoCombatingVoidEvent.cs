using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public class PureCombatingEventParam : IEventParameter
    {
        public PvMnBattleGeneralUnit Unit;
        public PvMnBattleGeneralUnit Target;

        private PureCombatingEventParam()
        {
            // Blocked
        }

        public PureCombatingEventParam(PvMnBattleGeneralUnit inOwner, PvMnBattleGeneralUnit inTarget)
        {
            Unit = inOwner;
            Target = inTarget;
        }
    }
    
    public interface ICombatTrackableVoidEvent
    {
        void Raise(PvMnBattleGeneralUnit eventOwner, PvMnBattleGeneralUnit target);
        void RegisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit> callback);
        void UnregisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit> callback);
    }

    public interface ICombatingOnStartEvent : ICombatTrackableVoidEvent
    {
        // Empty
    }

    public interface ICombatingTurnEndEvent : ICombatTrackableVoidEvent
    {
        // Empty
    }
    
    [CreateAssetMenu(fileName = "Combating Void Event", menuName = "ProjectCI Utilities/Events/Void/Combating Void Event")]
    public class PvSoCombatingVoidEvent : SoUnityEventBase<PureCombatingEventParam>, ICombatingOnStartEvent, ICombatingTurnEndEvent
    {
        private class TrackableVoidEvent
        {
            private readonly UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit> _originalEvent;
            internal bool IsRegistered;

            public TrackableVoidEvent(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit> originalEvent)
            {
                _originalEvent = originalEvent;
            }

            public void CallTranslatedCallback(IEventOwner owner, PureCombatingEventParam voidParam)
            {
                _originalEvent.Invoke(voidParam.Unit, voidParam.Target);
            }
        }

        private readonly PureCombatingEventParam _combatingEventParam = new(null, null);

        private readonly Dictionary<UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit>, TrackableVoidEvent>
            _translators = new();
        
        public void Raise(PvMnBattleGeneralUnit owner, PvMnBattleGeneralUnit target)
        {
            _combatingEventParam.Unit = owner;
            _combatingEventParam.Target = target;
            Raise(owner, _combatingEventParam);
        }
        
        public void RegisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit> callback)
        {
            if (!_translators.TryGetValue(callback, out var translator))
            {
                translator = new TrackableVoidEvent(callback);
                _translators.Add(callback, translator);
            }

            if (translator.IsRegistered)
            {
                return;
            }
            
            RegisterCallback(translator.CallTranslatedCallback);
            translator.IsRegistered = true;
        }

        public void UnregisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit> callback)
        {
            if (!_translators.TryGetValue(callback, out var translator))
            {
                return;
            }

            if (!translator.IsRegistered)
            {
                return;
            }
            
            UnregisterCallback(translator.CallTranslatedCallback);
            translator.IsRegistered = false;
        }
    }
}