using System;
using IndAssets.Scripts.Abilities;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    [Serializable]
    public struct PvEventDamageSourceInfo
    {
        public string FromId;
        public string ToId;
    }

    [Serializable]
    public struct PvEventDamageValueInfo
    {
        public int BeforeValue;
        public int AfterValue;
        public int DeltaValue;
    }

    [Serializable]
    public struct PvEventDamageTypeInfo
    {
        public PvEnDamageType Type;
        public PvEnDamageForm Form;
        public PvEnDamageReact Reaction;
    }

    public interface IPvDamageLikeApplyEvent
    {
        void Raise(string fromUnitID, string toUnitID,
            int beforeDamage, int afterDamage, int deltaValue,
            PvEnDamageType damageType, PvEnDamageForm damageForm, PvEnDamageReact hitInfo);
        void RegisterCallback(UnityAction<PvEventDamageSourceInfo, PvEventDamageValueInfo, PvEventDamageTypeInfo> callback);
        void RegisterCallbackVisually(UnityAction<PvEventDamageSourceInfo, PvEventDamageValueInfo, PvEventDamageTypeInfo> callback);
        void UnregisterCallback(UnityAction<PvEventDamageSourceInfo, PvEventDamageValueInfo, PvEventDamageTypeInfo> callback);
        void UnregisterCallbackVisually(UnityAction<PvEventDamageSourceInfo, PvEventDamageValueInfo, PvEventDamageTypeInfo> callback);
    }

    [CreateAssetMenu(fileName = "PvSoDamageLikeApplyEvent", menuName = "ProjectCI Utilities/Events/PvSoDamageLikeApplyEvent")]
    public class PvSoDamageLikeApplyEvent : SoUnityEventBase, IPvDamageLikeApplyEvent
    {
        private readonly UnityEvent<PvEventDamageSourceInfo, PvEventDamageValueInfo, PvEventDamageTypeInfo> _damageApplyEvent = new();
        private readonly UnityEvent<PvEventDamageSourceInfo, PvEventDamageValueInfo, PvEventDamageTypeInfo> _damageApplyVisualEvent = new();

        public void Raise(string fromUnitID, string toUnitID,
            int beforeDamage, int afterDamage, int deltaValue,
            PvEnDamageType damageType, PvEnDamageForm damageForm, PvEnDamageReact hitInfo)
        {
            var idInfo = new PvEventDamageSourceInfo
            {
                FromId = fromUnitID,
                ToId = toUnitID
            };

            var valueInfo = new PvEventDamageValueInfo
            {
                BeforeValue = beforeDamage,
                AfterValue = afterDamage,
                DeltaValue = deltaValue
            };

            var typeInfo = new PvEventDamageTypeInfo
            {
                Type = damageType,
                Form = damageForm,
                Reaction = hitInfo
            };

            _damageApplyEvent.Invoke(idInfo, valueInfo, typeInfo);
            _damageApplyVisualEvent.Invoke(idInfo, valueInfo, typeInfo);
        }

        public void RegisterCallback(UnityAction<PvEventDamageSourceInfo, PvEventDamageValueInfo, PvEventDamageTypeInfo> callback)
        {
            _damageApplyEvent.AddListener(callback);
        }

        public void RegisterCallbackVisually(UnityAction<PvEventDamageSourceInfo, PvEventDamageValueInfo, PvEventDamageTypeInfo> callback)
        {
            _damageApplyVisualEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<PvEventDamageSourceInfo, PvEventDamageValueInfo, PvEventDamageTypeInfo> callback)
        {
            _damageApplyEvent.RemoveListener(callback);
        }

        public void UnregisterCallbackVisually(UnityAction<PvEventDamageSourceInfo, PvEventDamageValueInfo, PvEventDamageTypeInfo> callback)
        {
            _damageApplyVisualEvent.RemoveListener(callback);
        }
    }
}
