using System;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public class UnitSelectEventParam : IEventParameter
    {
        public GridPawnUnit GridPawnUnit;
        public UnitSelectBehaviour Behaviour;
    }

    public enum UnitSelectBehaviour
    {
        Select,
        Deselect
    }
    
    [CreateAssetMenu(fileName = "UnitState Event", menuName = "ProjectCI Utilities/Events/UnitSelect Event")]
    public class PvSoUnitSelectEvent : SoUnityEventBase<UnitSelectEventParam>, IService, IEventOwner
    {
        private readonly UnitSelectEventParam _unitSelectParam = new();
        
        public void Raise(GridPawnUnit unit, UnitSelectBehaviour action)
        {
            _unitSelectParam.GridPawnUnit = unit;
            _unitSelectParam.Behaviour = action;
            Raise(this, _unitSelectParam);
        }

        public void Dispose()
        {
            Cleanup();
        }

        public void Initialize()
        {
            // Empty
        }

        public void Cleanup()
        {
            _unitSelectParam.GridPawnUnit = null;
            _unitSelectParam.Behaviour = UnitSelectBehaviour.Deselect;
            ClearCallbacks();
        }

        public string EventIdentifier => nameof(PvSoUnitSelectEvent);
    }
}