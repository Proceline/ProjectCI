using ProjectCI.Utilities.Runtime.Modifiers;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public interface IAttributeOwnerOnLogicallyKillEvent
    {
        void Raise(IAttributeOwner killer, IAttributeOwner victim);
        void RegisterCallback(UnityAction<IAttributeOwner, IAttributeOwner> callback);
        void UnregisterCallback(UnityAction<IAttributeOwner, IAttributeOwner> callback);
    }

    [CreateAssetMenu(fileName = "PvSoAttributeUnitPairEvent", menuName = "ProjectCI Utilities/Events/PvSoAttributeUnitPairEvent")]
    public class PvSoAttributeUnitPairEvent : SoUnityEventBase, IAttributeOwnerOnLogicallyKillEvent
    {
        private readonly UnityEvent<IAttributeOwner, IAttributeOwner> _onRuntimeEvent = new();

        public void Raise(IAttributeOwner fromOwner, IAttributeOwner toOwner)
        {
            _onRuntimeEvent?.Invoke(fromOwner, toOwner);
        }

        public void RegisterCallback(UnityAction<IAttributeOwner, IAttributeOwner> callback)
        {
            _onRuntimeEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<IAttributeOwner, IAttributeOwner> callback)
        {
            _onRuntimeEvent.RemoveListener(callback);
        }
    }
}