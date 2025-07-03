using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public struct VoidParameter : IEventParameter
    {
        public bool IsNull;
    }

    public class VoidEventTranslator
    {
        private readonly UnityAction _originalEvent;
        internal bool IsRegistered = false;

        public VoidEventTranslator(UnityAction originalEvent)
        {
            _originalEvent = originalEvent;
        }

        public void ReactLikeSoEvent(IEventOwner owner, VoidParameter voidParam)
        {
            _originalEvent.Invoke();
        }
    }

    public abstract class PvSoVoidEventBase : SoUnityEventBase<VoidParameter>, IEventOwner
    {
        private readonly Dictionary<UnityAction, VoidEventTranslator> _translators = new();
        
        public void Raise(IEventOwner owner)
        {
            Raise(owner, new VoidParameter());
        }

        public void Raise(IEventOwner owner, bool nullable)
        {
            Raise(owner, new VoidParameter { IsNull = nullable });
        }

        public void Raise()
        {
            Raise(this);
        }

        public void RegisterCallback(UnityAction callback)
        {
            if (!_translators.TryGetValue(callback, out VoidEventTranslator translator))
            {
                translator = new VoidEventTranslator(callback);
                _translators.Add(callback, translator);
            }

            if (translator.IsRegistered)
            {
                return;
            }
            
            RegisterCallback(translator.ReactLikeSoEvent);
            translator.IsRegistered = true;
        }

        public void UnregisterCallback(UnityAction callback)
        {
            if (!_translators.TryGetValue(callback, out VoidEventTranslator translator))
            {
                return;
            }

            if (!translator.IsRegistered)
            {
                return;
            }
            
            UnregisterCallback(translator.ReactLikeSoEvent);
            translator.IsRegistered = false;
        }

        public virtual void Dispose()
        {
            Cleanup();
        }

        public virtual void Initialize()
        {
            // Empty
        }

        public virtual void Cleanup()
        {
            ClearCallbacks();
            foreach (var translatorPair in _translators)
            {
                translatorPair.Value.IsRegistered = false;
            }
        }

        public string EventIdentifier => GetType().Name + "_" + name;
        public bool IsGridObject => false;
        public Vector3 Position => Vector3.zero;
        public Vector2 GridPosition => Vector2.zero;
    }
}