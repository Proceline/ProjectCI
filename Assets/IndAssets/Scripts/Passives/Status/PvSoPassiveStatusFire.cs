using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;

namespace IndAssets.Scripts.Passives.Status
{
    public class PvSoPassiveStatusFire : PvSoPassiveStatus
    {
        public override bool IsAccumulationAllowed { get; set; } = true;
        private readonly PvStatusData _dataPrefab = PvStatusData.CreateStatusData<PvSoPassiveStatusFire>(0, 0);

        protected override void InstallStatus(PvMnBattleGeneralUnit unit)
        {
            AccumulateStatus(unit, 1);
        }

        protected override void DisposeStatus(PvMnBattleGeneralUnit unit)
        {
            _dataPrefab.Layer = 100;
            RemoveStatusPrefab(unit, _dataPrefab);
        }

        protected override void AccumulateStatus(PvMnBattleGeneralUnit unit, int layer)
        {
            _dataPrefab.Layer = layer;
            AddStatusPrefab(unit, _dataPrefab);
        }

        protected override void ConsumeStatus(PvMnBattleGeneralUnit unit)
        {
            _dataPrefab.Layer = 1;
            RemoveStatusPrefab(unit, _dataPrefab);
        }
    }
}