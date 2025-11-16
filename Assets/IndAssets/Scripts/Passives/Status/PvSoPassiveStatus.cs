using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Status
{
    public abstract class PvSoPassiveStatus : ScriptableObject
    {
        [SerializeField] private Sprite statusIcon;
        public Sprite StatusIcon => statusIcon;
        
        public abstract void InstallStatus(GridPawnUnit unit);
        public abstract void DisposeStatus(GridPawnUnit unit);
        public abstract void AccumulateStatus(GridPawnUnit unit, int layer);
        public abstract void ConsumeStatus(GridPawnUnit unit);
        public abstract void OnStatusAppliedResponse(GridPawnUnit unit, IBattleStatus statusData);
        
        protected void AddStatusPrefab(GridPawnUnit unit, PvStatusData prefab)
        {
            unit.GetStatusEffectContainer().AddStatus(prefab);
        }
        
        protected void MarkStatusPrefabRemoved(GridPawnUnit unit, PvStatusData prefab)
        {
            unit.GetStatusEffectContainer().MarkDeductStatus(prefab);
        }
    }
}