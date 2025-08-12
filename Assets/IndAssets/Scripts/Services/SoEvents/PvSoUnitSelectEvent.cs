using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Events
{
    public class UnitSelectEventParam : IEventParameter
    {
        public PvMnBattleGeneralUnit Unit;
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
        
        public void Raise(PvMnBattleGeneralUnit unit, UnitSelectBehaviour action)
        {
            _unitSelectParam.Unit = unit;
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
            _unitSelectParam.Unit = null;
            _unitSelectParam.Behaviour = UnitSelectBehaviour.Deselect;
            ClearCallbacks();
        }

        public string EventIdentifier => nameof(PvSoUnitSelectEvent);
        public bool IsGridObject => false;
        public Vector3 Position => Vector3.zero;
        public Vector2 GridPosition => Vector2.zero;
    }
}