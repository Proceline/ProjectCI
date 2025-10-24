using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Status
{
    public class PvSoPassiveStatusFire : PvSoPassiveStatus
    {
        public override bool IsAccumulationAllowed { get; set; } = true;
        private readonly PvStatusData _dataPrefab = PvStatusData.CreateStatusData<PvSoPassiveStatusFire>(0, 0);

        [SerializeField] private int damagePerLayer;

        public override void InstallStatus(PvMnBattleGeneralUnit unit)
        {
            AccumulateStatus(unit, 1);
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
            _dataPrefab.Layer = 1;
            RemoveStatusPrefab(unit, _dataPrefab);
        }

        public override void OnStatusAppliedResponse(PvMnBattleGeneralUnit unit, IBattleStatus statusData)
        {
            var layerCount = statusData.Layer;
            var damageType = PvEnDamageType.Flame;
            var damage = layerCount * damagePerLayer;

            var container = unit.RuntimeAttributes;
            var beforeHealth = container.Health.CurrentValue;
            container.Health.ModifyValue(-damage);
            var afterHealth = container.Health.CurrentValue;

            // TODO: Consider Owner
            FeLiteGameRules.XRaiserSimpleDamageApplyEvent.Raise(beforeHealth, afterHealth, damage, unit,
                unit, damageType);

            ConsumeStatus(unit);
        }
    }
}