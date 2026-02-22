using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    
    [CreateAssetMenu(fileName = "UnitState Event", menuName = "ProjectCI Utilities/Events/Void/TurnViewEnd Event")]
    public sealed class PvSoTurnViewEndEvent : SoUnityEventBase
    {
        [SerializeField] private UnityEvent<bool> onTurnPreEndedEvent;

        private readonly UnityEvent<bool> _onTurnPostEndedEvent = new();

        public void RegisterCallback(UnityAction<bool> callback)
        {
            _onTurnPostEndedEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<bool> callback)
        {
            _onTurnPostEndedEvent.RemoveListener(callback);
        }

        public void Raise(bool started)
        {
            onTurnPreEndedEvent?.Invoke(started);
            _onTurnPostEndedEvent?.Invoke(started);
        }
    }
}