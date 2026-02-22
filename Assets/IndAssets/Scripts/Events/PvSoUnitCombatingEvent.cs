using System;
using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public interface IUnitCombatingEvent
    {
        /// <summary>
        /// Raise Event with specific step
        /// </summary>
        /// <param name="inUnit"></param>
        /// <param name="inTarget"></param>
        /// <param name="queryContexts"></param>
        void Raise(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<PvAbilityQueryItem<PvMnBattleGeneralUnit>> queryItems);
        void RegisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, List<PvAbilityQueryItem<PvMnBattleGeneralUnit>>> callback);
        void UnregisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, List<PvAbilityQueryItem<PvMnBattleGeneralUnit>>> callback);
    }

    public interface IUnitGeneralCombatingEvent : IUnitCombatingEvent
    {
        // Empty
    }

    public interface IUnitCombatingQueryEndEvent : IUnitCombatingEvent
    {
        // Empty
    }

    [CreateAssetMenu(fileName = "Unit Combating Event", menuName = "ProjectCI Utilities/Events/Unit Combating Event")]
    public class PvSoUnitCombatingEvent : SoUnityEventBase, IUnitGeneralCombatingEvent, IUnitCombatingQueryEndEvent
    {
        private readonly UnityEvent<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, 
            List<PvAbilityQueryItem<PvMnBattleGeneralUnit>>> _onRuntimeOnlyEvent = new();
        
        public void Raise(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<PvAbilityQueryItem<PvMnBattleGeneralUnit>> queryItems)
        {
            _onRuntimeOnlyEvent?.Invoke(inUnit, inTarget, queryItems);
        }

        public void RegisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, List<PvAbilityQueryItem<PvMnBattleGeneralUnit>>> callback)
        {
            _onRuntimeOnlyEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<PvMnBattleGeneralUnit, PvMnBattleGeneralUnit, List<PvAbilityQueryItem<PvMnBattleGeneralUnit>>> callback)
        {
            _onRuntimeOnlyEvent.RemoveListener(callback);
        }
    }
}