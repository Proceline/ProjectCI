using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    // [CreateAssetMenu(fileName = "SoUnityEventBase", menuName = "ProjectCI Utilities/Events/SoUnityEventBase")]
    public abstract class SoUnityEventBase : ScriptableObject
    {

    }

    public abstract class SoUnityEventBase<T> : SoUnityEventBase where T : IEventParameter
    {
        [SerializeField] private UnityEvent<IEventOwner, T> internalEvent;
        protected UnityEvent<IEventOwner, T> AccessibleEvent => internalEvent;

        public virtual void RegisterCallback(UnityAction<IEventOwner, T> callback)
        {
            internalEvent.AddListener(callback);
        }

        public virtual void UnregisterCallback(UnityAction<IEventOwner, T> callback)
        {
            internalEvent.RemoveListener(callback);
        }

        public void ClearCallbacks()
        {
            internalEvent.RemoveAllListeners();
        }

        public virtual void Raise(IEventOwner owner, T value)
        {
            internalEvent?.Invoke(owner, value);
        }
    }
}