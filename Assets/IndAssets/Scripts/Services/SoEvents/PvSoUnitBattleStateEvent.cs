using System;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Events
{
    [Serializable]
    public struct UnitStateEventParam : IEventParameter
    {
        public UnitBattleState battleState;
        public UnitStateBehaviour behaviour;
    }

    public enum UnitStateBehaviour
    {
        Clear,
        Adding,
        Popping,
        Emphasis
    }
    
    [CreateAssetMenu(fileName = "UnitState Event", menuName = "ProjectCI Utilities/Events/UnitBattleState Event")]
    public class PvSoUnitBattleStateEvent : SoUnityEventBase<UnitStateEventParam>
    {
        public void Raise(IEventOwner unit, UnitBattleState pureValue, UnitStateBehaviour action)
        {
            Raise(unit, new UnitStateEventParam { battleState = pureValue, behaviour = action });
        }

        public void Dispose()
        {
            ClearCallbacks();
        }

        public void Initialize()
        {
            // Empty
        }

        public void Cleanup()
        {
            ClearCallbacks();
        }
    }
}