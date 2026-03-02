using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectCI.Utilities.Runtime.Events
{
    public interface IOnStatusApplyEvent
    {
        void Raise(GridPawnUnit targetUnit, string passiveName);
        void RegisterCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback);
        void UnregisterCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback);
    }
    
    [CreateAssetMenu(fileName = "PvSoStatusApplyEvent", menuName = "ProjectCI Utilities/Events/PvSoStatusApplyEvent")]
    public class PvSoStatusApplyEvent : SoUnityEventBase, IOnStatusApplyEvent
    {
        [SerializeField]
        private List<PvSoPassiveBase> allRegisteredPassives;

        private readonly Dictionary<string, PvSoPassiveBase> _bufferedPassive = new();

        [SerializeField]
        private UnityEvent<GridPawnUnit, PvSoPassiveBase> onRuntimePreInstalledEvents;
        private readonly UnityEvent<GridPawnUnit, PvSoPassiveBase> _onRuntimePostEvent = new();

        public void Raise(GridPawnUnit targetUnit, string passiveAssetName)
        {
            if (!_bufferedPassive.TryGetValue(passiveAssetName, out var passive))
            {
                passive = allRegisteredPassives.Find(item => item.name == passiveAssetName);
                _bufferedPassive.Add(passiveAssetName, passive);
            }

            if (passive)
            {
                onRuntimePreInstalledEvents?.Invoke(targetUnit, passive);
                _onRuntimePostEvent?.Invoke(targetUnit, passive);
            }
        }

        public void RegisterCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback)
        {
            _onRuntimePostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback)
        {
            _onRuntimePostEvent.RemoveListener(callback);
        }
    }
}