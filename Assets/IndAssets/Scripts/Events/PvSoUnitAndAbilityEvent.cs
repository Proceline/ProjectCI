using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    [Serializable]
    public class UnitAndAbilityEventParam : IEventParameter
    {
        public PvMnBattleGeneralUnit unit;
        public PvSoUnitAbility ability;
        public PvMnBattleGeneralUnit target;
        public Queue<CommandResult> ResultsReference { get; internal set; }
    }

    public interface IUnitAndAbilityEvent
    {
        void Raise(PvMnBattleGeneralUnit inUnit, PvSoUnitAbility inAbility, PvMnBattleGeneralUnit target,
            Queue<CommandResult> existedResults);
        void RegisterCallback(UnityAction<IEventOwner, UnitAndAbilityEventParam> callback);
        void UnregisterCallback(UnityAction<IEventOwner, UnitAndAbilityEventParam> callback);
    }

    public interface IUnitCombatLogicFinishedEvent : IUnitAndAbilityEvent
    {
        // Empty: For Injection
    }

    public interface IUnitCombatLogicPreEvent : IUnitAndAbilityEvent
    {
        // Empty: For Injection
    }

    [CreateAssetMenu(fileName = "Unit&Ability Event", menuName = "ProjectCI Utilities/Events/Unit and Ability Event")]
    public class PvSoUnitAndAbilityEvent : SoUnityEventBase<UnitAndAbilityEventParam>, IUnitCombatLogicFinishedEvent,
        IUnitCombatLogicPreEvent
    {
        [NonSerialized] private UnitAndAbilityEventParam _bufferedParam;
        [NonSerialized] private bool _hasEverBuffered;

        public void Raise(PvMnBattleGeneralUnit inUnit, PvSoUnitAbility inAbility, PvMnBattleGeneralUnit inTarget,
            Queue<CommandResult> existedResults)
        {
            if (!_hasEverBuffered)
            {
                _bufferedParam = new UnitAndAbilityEventParam
                    { unit = inUnit, ability = inAbility, target = inTarget, ResultsReference = existedResults };
                _hasEverBuffered = true;
            }
            else
            {
                _bufferedParam.unit = inUnit;
                _bufferedParam.ability = inAbility;
                _bufferedParam.target = inTarget;
                _bufferedParam.ResultsReference = existedResults;
            }

            Raise(inUnit, _bufferedParam);
        }
    }
}