using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ProjectCI.Utilities.Runtime.Events
{
    [CreateAssetMenu(fileName = "PvSoNotifyDamageBeforeRevEvent", menuName = "ProjectCI Utilities/Events/PvSoNotifyDamageBeforeRevEvent")]
    public class PvSoNotifyDamageBeforeRevEvent : SoUnityEventBase
    {
        /// <summary>
        /// Param_0: int[], damage before adjusted
        /// Param_1: GridPawnUnit, the unit who received damage
        /// Param_2: GridPawnUnit, the unit who caused damage
        /// Param_3: uint, possible Enum of damage type
        /// </summary>
        private readonly UnityEvent<int[], GridPawnUnit, GridPawnUnit, uint> _onRuntimeEvent = new();
        private event Action<int[], GridPawnUnit, GridPawnUnit, uint> _onRuntimePostEvent;

        [NonSerialized] private int[] _allocatedValues = new int[1];

        public int Raise(int inputValue, GridPawnUnit victim, GridPawnUnit owner, uint extraInfo)
        {
            _allocatedValues[0] = inputValue;
            _onRuntimeEvent?.Invoke(_allocatedValues, victim, owner, extraInfo);
            _onRuntimePostEvent?.Invoke(_allocatedValues, victim, owner, extraInfo);
            return _allocatedValues[0];
        }

        public void RegisterCallback(UnityAction<int[], GridPawnUnit, GridPawnUnit, uint> callback)
        {
            _onRuntimeEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<int[], GridPawnUnit, GridPawnUnit, uint> callback)
        {
            _onRuntimeEvent.RemoveListener(callback);
        }

        public void RegisterPostCallback(Action<int[], GridPawnUnit, GridPawnUnit, uint> callback)
        {
            _onRuntimePostEvent += callback;
        }

        public void UnregisterPostCallback(Action<int[], GridPawnUnit, GridPawnUnit, uint> callback)
        {
            _onRuntimePostEvent -= callback;
        }
    }
}
