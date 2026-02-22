using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    [CreateAssetMenu(fileName = "PvSoEquipEntryEvent", menuName = "ProjectCI Utilities/Events/PvSoEquipEntryEvent")]
    public class PvSoEquipEntryEvent : SoUnityEventBase
    {
        [SerializeField]
        private UnityEvent<string, string> onPreInstalledEvent;
        private readonly UnityEvent<string, string> _onPostEvent = new();

        
        public void Raise(string weaponInstanceId, string characterId)
        {
            onPreInstalledEvent?.Invoke(weaponInstanceId, characterId);
            _onPostEvent?.Invoke(weaponInstanceId, characterId);
        }

        public void RegisterCallback(UnityAction<string, string> callback)
        {
            _onPostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<string, string> callback)
        {
            _onPostEvent.RemoveListener(callback);
        }
    }
}