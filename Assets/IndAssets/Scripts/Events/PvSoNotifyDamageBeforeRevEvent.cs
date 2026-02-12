using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Events
{
    [CreateAssetMenu(fileName = "PvSoNotifyDamageBeforeRevEvent", menuName = "ProjectCI Utilities/Events/PvSoNotifyDamageBeforeRevEvent")]
    public class PvSoNotifyDamageBeforeRevEvent : SoUnityEventBase
    {
        /// <summary>
        /// Param_0: int[], damage before adjusted
        /// Param_1: string, uniqueId for the damage instance, can be used to link with other events
        /// Param_2: GridPawnUnit, the unit who received damage
        /// Param_3: GridPawnUnit, the unit who caused damage
        /// Param_4: uint, possible Enum of damage type
        /// </summary>
        private Func<string, GridPawnUnit, GridPawnUnit, uint, int> _onRuntimeEvent;
        private event Action<int[], string, GridPawnUnit, GridPawnUnit, uint> _onRuntimePostEvent;

        [NonSerialized] private int[] _allocatedValues = new int[1];

        public int Raise(int inputValue, string uniqueId, GridPawnUnit victim, GridPawnUnit owner, uint extraInfo)
        {
            _allocatedValues[0] = inputValue;
            var result = _onRuntimeEvent == null? 0 : _onRuntimeEvent.Invoke(uniqueId, victim, owner, extraInfo);
            _onRuntimePostEvent?.Invoke(_allocatedValues, uniqueId, victim, owner, extraInfo);
            var output = result + _allocatedValues[0];
            return output;
        }

        public void RegisterCallback(Func<string, GridPawnUnit, GridPawnUnit, uint, int> callback)
        {
            _onRuntimeEvent += callback;
        }

        public void UnregisterCallback(Func<string, GridPawnUnit, GridPawnUnit, uint, int> callback)
        {
            _onRuntimeEvent -= callback;
        }

        public void RegisterPostCallback(Action<int[], string, GridPawnUnit, GridPawnUnit, uint> callback)
        {
            _onRuntimePostEvent += callback;
        }

        public void UnregisterPostCallback(Action<int[], string, GridPawnUnit, GridPawnUnit, uint> callback)
        {
            _onRuntimePostEvent -= callback;
        }
    }
}
