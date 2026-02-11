using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    [CreateAssetMenu(fileName = "PvSoAnimationTriggerEvent", menuName = "ProjectCI Utilities/Events/PvSoAnimationTriggerEvent")]
    public class PvSoAnimationTriggerEvent : SoUnityEventBase
    {
        private readonly Dictionary<Object, UnityEvent<string>> _preloadedEvents = new();

        public void Raise(Object owner, string name)
        {
            if (_preloadedEvents.TryGetValue(owner, out var unityEvent))
            {
                unityEvent.Invoke(name);
            }
        }

        public void RegisterCallback(Object owner, UnityAction<string> callback)
        {
            if (!_preloadedEvents.TryGetValue(owner, out var unityEvent))
            {
                unityEvent = new();
                _preloadedEvents[owner] = unityEvent;
            }
            unityEvent.RemoveAllListeners();
            unityEvent.AddListener(callback);
        }

        public void UnregisterCallback(Object owner)
        {
            if (_preloadedEvents.TryGetValue(owner, out var unityEvent))
            {
                unityEvent.RemoveAllListeners();
                _preloadedEvents.Remove(owner);
            }
        }
    }
}