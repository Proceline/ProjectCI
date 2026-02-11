using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public interface IAnimationOutLengthFunc
    {
        float Raise(Object owner, string name);
        void RegisterCallback(Object owner, UnityAction<float[], string> callback);
        void UnregisterCallback(Object owner);
    }

    public interface IAnimationOutBreakPointFunc : IAnimationOutLengthFunc
    {
        // Empty
    }

    [CreateAssetMenu(fileName = "PvSoAnimationOutDataEvent", menuName = "ProjectCI Utilities/Events/PvSoAnimationOutDataEvent")]
    public class PvSoAnimationOutDataEvent : SoUnityEventBase, IAnimationOutLengthFunc, IAnimationOutBreakPointFunc
    {
        private readonly Dictionary<Object, float> _bufferedOutputs = new();
        private readonly float[] _raiserUsedValueAlloc = new float[1];
        private readonly Dictionary<Object, UnityEvent<float[], string>> _preloadedEvents = new();

        public float Raise(Object owner, string name)
        {
            _raiserUsedValueAlloc[0] = 0;
            if (_preloadedEvents.TryGetValue(owner, out var unityEvent))
            {
                unityEvent.Invoke(_raiserUsedValueAlloc, name);
            }
            _bufferedOutputs[owner] = _raiserUsedValueAlloc[0];
            return _bufferedOutputs[owner];
        }

        public void RegisterCallback(Object owner, UnityAction<float[], string> callback)
        {
            if (!_preloadedEvents.TryGetValue(owner, out var unityEvent))
            {
                unityEvent = new();
                _preloadedEvents[owner] = unityEvent;
            }
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