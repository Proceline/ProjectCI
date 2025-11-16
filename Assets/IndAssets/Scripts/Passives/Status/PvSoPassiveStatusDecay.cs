using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace IndAssets.Scripts.Passives.Status
{
    public class PvSoPassiveStatusDecay : PvSoPassiveStatus
    {
        private readonly PvStatusData _dataPrefab = PvStatusData.CreateStatusData<PvSoPassiveStatusDecay>(1, 0);

        public override void InstallStatus(GridPawnUnit unit)
        {
            AccumulateStatus(unit, 0);
        }

        public override void DisposeStatus(GridPawnUnit unit)
        {
            _dataPrefab.Layer = 100;
            MarkStatusPrefabRemoved(unit, _dataPrefab);
        }

        public override void AccumulateStatus(GridPawnUnit unit, int layer)
        {
            _dataPrefab.Layer = layer;
            AddStatusPrefab(unit, _dataPrefab);
        }

        public override void ConsumeStatus(GridPawnUnit unit)
        {
            DisposeStatus(unit);
        }

        public override void OnStatusAppliedResponse(GridPawnUnit unit, IBattleStatus statusData)
        {
            throw new System.NotImplementedException();
        }
    }
}