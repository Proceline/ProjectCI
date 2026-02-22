using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Utilities.Runtime.Events
{
    [CreateAssetMenu(fileName = "LevelCellBase Event", menuName = "ProjectCI Utilities/Events/LevelCell Event")]
    public class PvSoLevelCellEvent : SoUnityEventBase
    {
        [SerializeField]
        private UnityEvent<LevelCellBase> onRuntimePreInstalledEvents;

        private readonly UnityEvent<LevelCellBase> _onRuntimePostEvent = new();

        public void Raise(LevelCellBase cell)
        {
            onRuntimePreInstalledEvents?.Invoke(cell);
            _onRuntimePostEvent?.Invoke(cell);
        }

        public void RegisterCallback(UnityAction<LevelCellBase> callback)
        {
            _onRuntimePostEvent.AddListener(callback);
        }

        public void UnregisterCallback(UnityAction<LevelCellBase> callback)
        {
            _onRuntimePostEvent.RemoveListener(callback);
        }
    }
}