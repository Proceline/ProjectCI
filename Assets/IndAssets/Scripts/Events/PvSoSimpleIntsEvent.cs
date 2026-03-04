using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public interface IEnergyUpdateEvent
    {
        void Raise(string ownerId, params int[] energyValues);
        void RegisterCallbackLogically(UnityAction<string, int[]> callback);
        void UnregisterCallbackLogically(UnityAction<string, int[]> callback);
        void RegisterCallbackVisually(UnityAction<string, int[]> callback);
        void UnregisterCallbackVisually(UnityAction<string, int[]> callback);
    }

    [CreateAssetMenu(fileName = "PvSoSimpleIntsEvent", menuName = "ProjectCI Utilities/Events/PvSoSimpleIntsEvent")]
    public class PvSoSimpleIntsEvent : SoUnityEventBase, IEnergyUpdateEvent
    {
        private readonly UnityEvent<string, int[]> _onRuntimeEvent = new();
        private readonly UnityEvent<string, int[]> _onRuntimeVisualEvent = new();
        
        public void Raise(string ownerId, params int[] values)
        {
            _onRuntimeEvent?.Invoke(ownerId, values);
            _onRuntimeVisualEvent?.Invoke(ownerId, values);
        }

        public void RegisterCallbackLogically(UnityAction<string, int[]> callback)
        {
            _onRuntimeEvent.AddListener(callback);
        }

        public void UnregisterCallbackLogically(UnityAction<string, int[]> callback)
        {
            _onRuntimeEvent.RemoveListener(callback);
        }

        public void RegisterCallbackVisually(UnityAction<string, int[]> callback)
        {
            _onRuntimeVisualEvent.AddListener(callback);
        }

        public void UnregisterCallbackVisually(UnityAction<string, int[]> callback)
        {
            _onRuntimeVisualEvent.RemoveListener(callback);
        }

    }
}