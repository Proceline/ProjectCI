using System;
using System.Collections.Generic;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.Utilities.Runtime.Functions
{
    public abstract class SoScriptableFunctionBase<T, TP> : ScriptableObject where TP : IEventParameter
    {
        private readonly IDictionary<IEventOwner, Func<IEventOwner, TP, T>> _registeredFunctions =
            new Dictionary<IEventOwner, Func<IEventOwner, TP, T>>();

        public void RegisterDelegate(IEventOwner eventOwner, Func<IEventOwner, TP, T> func)
        {
            if (!_registeredFunctions.TryAdd(eventOwner, func))
            {
                Debug.LogWarning("Warning: Scriptable Function ONLY allows ONE function for each IEventOwner");
            }
        }

        public void UnregisterDelegate(IEventOwner eventOwner)
        {
            _registeredFunctions.Remove(eventOwner);
        }

        public void ClearDelegate()
        {
            _registeredFunctions.Clear();
        }

        protected bool Exists(IEventOwner owner) => _registeredFunctions.ContainsKey(owner);

        public virtual T Get(IEventOwner owner, TP parameter)
        {
            return _registeredFunctions.TryGetValue(owner, out var func) ? func.Invoke(owner, parameter) : default;
        }
    }
    
    public struct EmptyParameterForFunction : IEventParameter
    {
        // Empty
    }
    
    public abstract class SoScriptableFunctionBase<T> : SoScriptableFunctionBase<T, EmptyParameterForFunction>
    {
        public virtual T Get(IEventOwner owner) => Get(owner, new EmptyParameterForFunction());
    }
}