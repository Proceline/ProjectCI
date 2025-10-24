using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;

namespace IndAssets.Scripts.Passives.Status
{
    public class PvSoPassiveStatusDecay : PvSoPassiveStatus
    {
        public override bool IsAccumulationAllowed { get; set; } = true;
        private readonly PvStatusData _dataPrefab = PvStatusData.CreateStatusData<PvSoPassiveStatusDecay>(1, 0);

        protected override void InstallStatus(PvMnBattleGeneralUnit unit)
        {
            AccumulateStatus(unit, 0);
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
            DisposeStatus(unit);
        }
    }
}