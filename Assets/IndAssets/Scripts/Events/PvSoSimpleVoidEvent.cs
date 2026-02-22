using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{   
    [CreateAssetMenu(fileName = "PvSoSimpleVoidEvent", menuName = "ProjectCI Utilities/Events/PvSoSimpleVoidEvent")]
    public class PvSoSimpleVoidEvent : SoUnityEventBase
    {
        [SerializeField]
        private UnityEvent onRuntimePreInstalledEvents;

        private readonly UnityEvent _onRuntimePostEvent = new();
        
        public void Raise()
        {
            onRuntimePreInstalledEvents?.Invoke();
            _onRuntimePostEvent?.Invoke();
        }

        public void RegisterCallback(UnityAction callback)
        {
            _onRuntimePostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction callback)
        {
            _onRuntimePostEvent.RemoveListener(callback);
        }
    }
}