using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Status
{
    [CreateAssetMenu(fileName = "New Fire Status", menuName = "ProjectCI Status/FireBurn", order = 1)]
    public class PvSoPassiveStatusFire : PvSoPassiveStatus
    {
        private readonly PvStatusData _dataPrefab = PvStatusData.CreateStatusData<PvSoPassiveStatusFire>(0, 0);

        [SerializeField] private int damagePerLayer;

        public override void InstallStatus(GridPawnUnit unit)
        {
            AccumulateStatus(unit, 1);
        }

        public override void DisposeStatus(GridPawnUnit unit)
        {
            // Reduce 100 Layer, no Duration to status list
            _dataPrefab.Layer = 100;
            _dataPrefab.Duration = 0;
            RemoveStatusPrefab(unit, _dataPrefab);
        }

        public override void AccumulateStatus(GridPawnUnit unit, int layer)
        {
            Debug.Log($"<{nameof(PvSoPassiveStatusFire)}>: Status Accumulated!");
            _dataPrefab.Layer = layer;
            _dataPrefab.Duration = 1;
            AddStatusPrefab(unit, _dataPrefab);
        }

        public override void ConsumeStatus(GridPawnUnit unit)
        {
            // Reduce 1 Layer, no Duration to status list
            _dataPrefab.Layer = 1;
            _dataPrefab.Duration = 0;
            
            // Reduce one Layer on Unit
            RemoveStatusPrefab(unit, _dataPrefab);
        }

        public override void OnStatusAppliedResponse(GridPawnUnit unit, IBattleStatus statusData)
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