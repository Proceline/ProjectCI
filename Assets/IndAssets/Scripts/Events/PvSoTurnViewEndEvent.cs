using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    
    [CreateAssetMenu(fileName = "UnitState Event", menuName = "ProjectCI Utilities/Events/Void/TurnViewEnd Event")]
    public sealed class PvSoTurnViewEndEvent : SoUnityEventBase
    {
        [SerializeField] private UnityEvent onTurnPreEndedEvent;

        private readonly UnityEvent _onTurnPostEndedEvent = new();

        public void RegisterCallback(UnityAction callback)
        {
            _onTurnPostEndedEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction callback)
        {
            _onTurnPostEndedEvent.RemoveListener(callback);
        }

        public void Raise()
        {
            onTurnPreEndedEvent?.Invoke();
            _onTurnPostEndedEvent?.Invoke();
        }
    }
}