using System;
using System.Collections.Generic;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectCI.Utilities.Runtime.Functions
{
    
    [CreateAssetMenu(fileName = "Boolean OutFunction", menuName = "ProjectCI Utilities/Functions/Boolean OutFunction")]
    public class PvSoOutBooleanFunction : SoScriptableFunctionBase<bool>
    {
        [SerializeField] private bool defaultValueForEmpty;

        private readonly Dictionary<int, IEventOwner> _ownersRecord = new();

        public void RegisterDelegate<T>(T eventOwner, Func<IEventOwner, EmptyParameterForFunction, bool> func) where T : Object, IEventOwner
        {
            _ownersRecord.TryAdd(eventOwner.GetInstanceID(), eventOwner);
            base.RegisterDelegate(eventOwner, func);
        }

        public override bool Get(IEventOwner owner)
        {
            return Exists(owner) ? base.Get(owner) : defaultValueForEmpty;
        }

        public bool Get(Object ownerObject)
        {
            var instanceId = ownerObject.GetInstanceID();
            return _ownersRecord.TryGetValue(instanceId, out var value) ? Get(value) : defaultValueForEmpty;
        }
    }
}