using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    public interface IOnStatusApplyEvent
    {
        void Raise(GridPawnUnit targetUnit, string passiveName);
        void Dispose(GridPawnUnit targetUnit, string passiveName);
        void RegisterCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback);
        void RegisterVisualCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback);
        void UnregisterCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback);
        void UnregisterVisualCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback);

        void RegisterUnsetCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback);
        void RegisterUnsetVisualCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback);
        void UnregisterUnsetCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback);
        void UnregisterUnsetVisualCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback);
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
        private readonly UnityEvent<GridPawnUnit, PvSoPassiveBase> _onVisualEvent = new();

        [SerializeField]
        private UnityEvent<GridPawnUnit, PvSoPassiveBase> onPreUninstalledEvents;
        private readonly UnityEvent<GridPawnUnit, PvSoPassiveBase> _onRuntimeUninstallEvent = new();
        private readonly UnityEvent<GridPawnUnit, PvSoPassiveBase> _onVisualUninstallEvent = new();

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
                _onVisualEvent?.Invoke(targetUnit, passive);
            }
        }

        public void Dispose(GridPawnUnit targetUnit, string passiveAssetName)
        {
            if (!_bufferedPassive.TryGetValue(passiveAssetName, out var passive))
            {
                passive = allRegisteredPassives.Find(item => item.name == passiveAssetName);
                _bufferedPassive.Add(passiveAssetName, passive);
            }

            if (passive)
            {
                onPreUninstalledEvents?.Invoke(targetUnit, passive);
                _onRuntimeUninstallEvent?.Invoke(targetUnit, passive);
                _onVisualUninstallEvent?.Invoke(targetUnit, passive);
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

        public void RegisterVisualCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback)
        {
            _onVisualEvent.AddListener(callback);
        }

        public void UnregisterVisualCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback)
        {
            _onVisualEvent.RemoveListener(callback);
        }

        public void RegisterUnsetCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback)
        {
            _onRuntimeUninstallEvent.AddListener(callback);
        }
        public void UnregisterUnsetCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback)
        {
            _onRuntimeUninstallEvent.RemoveListener(callback);
        }

        public void RegisterUnsetVisualCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback)
        {
            _onVisualUninstallEvent.AddListener(callback);
        }

        public void UnregisterUnsetVisualCallback(UnityAction<GridPawnUnit, PvSoPassiveBase> callback)
        {
            _onVisualUninstallEvent.RemoveListener(callback);
        }
    }
}