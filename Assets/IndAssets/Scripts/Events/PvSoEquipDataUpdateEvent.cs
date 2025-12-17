using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    [CreateAssetMenu(fileName = "PvSoEquipDataUpdateEvent", menuName = "ProjectCI Utilities/Events/PvSoEquipDataUpdateEvent")]
    public class PvSoEquipDataUpdateEvent : SoUnityEventBase
    {
        [SerializeField]
        private UnityEvent<List<string>, List<string>, Dictionary<string, string>> onPreInstalledEvent;
        private readonly UnityEvent<List<string>, List<string>, Dictionary<string, string>> _onPostEvent = new();

        
        public void Raise(List<string> availableInstanceIds, List<string> availableDisplayNames, Dictionary<string, string> equippedInstanceInfos)
        {
            onPreInstalledEvent?.Invoke(availableInstanceIds, availableDisplayNames, equippedInstanceInfos);
            _onPostEvent?.Invoke(availableInstanceIds, availableDisplayNames, equippedInstanceInfos);
        }

        public void RegisterCallback(UnityAction<List<string>, List<string>, Dictionary<string, string>> callback)
        {
            _onPostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<List<string>, List<string>, Dictionary<string, string>> callback)
        {
            _onPostEvent.RemoveListener(callback);
        }
    }
}