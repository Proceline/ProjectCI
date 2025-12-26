using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    [CreateAssetMenu(fileName = "PvSoEquipEntryEvent", menuName = "ProjectCI Utilities/Events/PvSoEquipEntryEvent")]
    public class PvSoEquipEntryEvent : SoUnityEventBase
    {
        [SerializeField]
        private UnityEvent<string, string, int> onPreInstalledEvent;
        private readonly UnityEvent<string, string, int> _onPostEvent = new();

        
        public void Raise(string weaponInstanceId, string characterId, int slotIndex)
        {
            onPreInstalledEvent?.Invoke(weaponInstanceId, characterId, slotIndex);
            _onPostEvent?.Invoke(weaponInstanceId, characterId, slotIndex);
        }

        public void RegisterCallback(UnityAction<string, string, int> callback)
        {
            _onPostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<string, string, int> callback)
        {
            _onPostEvent.RemoveListener(callback);
        }
    }
}