using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;

namespace IndAssets.Scripts.Passives.Status
{
    public class PvSoPassiveStatusDecay : PvSoPassiveStatus
    {
        private readonly PvStatusData _dataPrefab = PvStatusData.CreateStatusData<PvSoPassiveStatusDecay>(1, 0);

        public override void InstallStatus(PvMnBattleGeneralUnit unit)
        {
            AccumulateStatus(unit, 0);
        }

        public override void DisposeStatus(PvMnBattleGeneralUnit unit)
        {
            _dataPrefab.Layer = 100;
            RemoveStatusPrefab(unit, _dataPrefab);
        }

        public override void AccumulateStatus(PvMnBattleGeneralUnit unit, int layer)
        {
            _dataPrefab.Layer = layer;
            AddStatusPrefab(unit, _dataPrefab);
        }

        public override void ConsumeStatus(PvMnBattleGeneralUnit unit)
        {
            DisposeStatus(unit);
        }

        public override void OnStatusAppliedResponse(PvMnBattleGeneralUnit unit, IBattleStatus statusData)
        {
            throw new System.NotImplementedException();
        }
    }
}