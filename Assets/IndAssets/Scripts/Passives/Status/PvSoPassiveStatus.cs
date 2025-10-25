using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Status
{
    public abstract class PvSoPassiveStatus : ScriptableObject
    {
        public abstract void InstallStatus(PvMnBattleGeneralUnit unit);
        public abstract void DisposeStatus(PvMnBattleGeneralUnit unit);
        public abstract void AccumulateStatus(PvMnBattleGeneralUnit unit, int layer);
        public abstract void ConsumeStatus(PvMnBattleGeneralUnit unit);
        public abstract void OnStatusAppliedResponse(PvMnBattleGeneralUnit unit, IBattleStatus statusData);
        
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