using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public enum UnitSelectBehaviour
    {
        Select,
        Deselect
    }

    [CreateAssetMenu(fileName = "UnitState Event", menuName = "ProjectCI Utilities/Events/UnitSelect Event")]
    public class PvSoUnitSelectEvent : SoUnityEventBase
    {
        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit, UnitSelectBehaviour> onRuntimePreInstalledEvents;

        private readonly UnityEvent<PvMnBattleGeneralUnit, UnitSelectBehaviour> _onRuntimePostEvent = new();

        public void Raise(PvMnBattleGeneralUnit unit, UnitSelectBehaviour action)
        {
            onRuntimePreInstalledEvents?.Invoke(unit, action);
            _onRuntimePostEvent?.Invoke(unit, action);
        }

        public void RegisterCallback(UnityAction<PvMnBattleGeneralUnit, UnitSelectBehaviour> callback)
        {
            _onRuntimePostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<PvMnBattleGeneralUnit, UnitSelectBehaviour> callback)
        {
            _onRuntimePostEvent.RemoveListener(callback);
        }
    }
}