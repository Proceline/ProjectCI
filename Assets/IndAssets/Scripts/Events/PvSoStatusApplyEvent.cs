using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectCI.Utilities.Runtime.Events
{
    public interface IOnStatusApplyEvent
    {
        Image StatusViewPrefab { get; }
        void Raise(GridPawnUnit targetUnit, PvSoPassiveStatus statusType);
        void RegisterCallback(UnityAction<GridPawnUnit, PvSoPassiveStatus> callback);
        void UnregisterCallback(UnityAction<GridPawnUnit, PvSoPassiveStatus> callback);
    }
    
    [CreateAssetMenu(fileName = "PvSoStatusApplyEvent", menuName = "ProjectCI Utilities/Events/PvSoStatusApplyEvent")]
    public class PvSoStatusApplyEvent : SoUnityEventBase, IOnStatusApplyEvent
    {
        [SerializeField]
        private UnityEvent<GridPawnUnit, PvSoPassiveStatus> onRuntimePreInstalledEvents;
        private readonly UnityEvent<GridPawnUnit, PvSoPassiveStatus> _onRuntimePostEvent = new();

        [SerializeField]
        private Image statusViewPrefab;
        public Image StatusViewPrefab => statusViewPrefab;

        
        public void Raise(GridPawnUnit targetUnit, PvSoPassiveStatus statusType)
        {
            onRuntimePreInstalledEvents?.Invoke(targetUnit, statusType);
            _onRuntimePostEvent?.Invoke(targetUnit, statusType);
        }

        public void RegisterCallback(UnityAction<GridPawnUnit, PvSoPassiveStatus> callback)
        {
            _onRuntimePostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<GridPawnUnit, PvSoPassiveStatus> callback)
        {
            _onRuntimePostEvent.RemoveListener(callback);
        }
    }
}