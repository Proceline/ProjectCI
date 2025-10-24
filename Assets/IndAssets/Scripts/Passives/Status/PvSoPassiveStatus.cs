using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Status
{
    public abstract class PvSoPassiveStatus : ScriptableObject
    {
        public virtual bool IsAccumulationAllowed { get; set; } = false;
        public abstract void InstallStatus(PvMnBattleGeneralUnit unit);
        public abstract void DisposeStatus(PvMnBattleGeneralUnit unit);
        public abstract void AccumulateStatus(PvMnBattleGeneralUnit unit, int layer);
        public abstract void ConsumeStatus(PvMnBattleGeneralUnit unit);
        
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