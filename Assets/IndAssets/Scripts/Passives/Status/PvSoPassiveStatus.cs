using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Status
{
    public abstract class PvSoPassiveStatus : ScriptableObject
    {
        public virtual bool IsAccumulationAllowed { get; set; } = false;
        protected abstract void InstallStatus(PvMnBattleGeneralUnit unit);
        protected abstract void DisposeStatus(PvMnBattleGeneralUnit unit);
        protected abstract void AccumulateStatus(PvMnBattleGeneralUnit unit, int layer);
        protected abstract void ConsumeStatus(PvMnBattleGeneralUnit unit);
        
        protected void AddStatusPrefab(PvMnBattleGeneralUnit unit, PvStatusData prefab)
        {
            unit.GetStatusEffectContainer().AddStatus(prefab);
        }
        
        protected void RemoveStatusPrefab(PvMnBattleGeneralUnit unit, PvStatusData prefab)
        {
            unit.GetStatusEffectContainer().RemoveStatus(prefab);
        }
    }
}