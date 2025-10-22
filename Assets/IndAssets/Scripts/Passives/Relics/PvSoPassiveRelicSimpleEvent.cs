using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.Events;

namespace IndAssets.Scripts.Passives.Relics
{
    [CreateAssetMenu(fileName = "SoRelic_SimpleEventTBD", menuName = "ProjectCI Passives/Relics/SimpleEvent", order = 1)]
    public class PvSoPassiveRelicSimpleEvent : PvSoPassiveRelic
    {
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onRelicSettledForUnit;
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onRelicWithdrawFromUnit;
        
        protected override void InstallPassiveInternally(GridPawnUnit unit)
        {
            if (unit is PvMnBattleGeneralUnit battleUnit)
            {
                onRelicSettledForUnit.Invoke(battleUnit);
            }
        }

        protected override void DisposePassiveInternally(GridPawnUnit unit)
        {
            if (unit is PvMnBattleGeneralUnit battleUnit)
            {
                onRelicWithdrawFromUnit.Invoke(battleUnit);
            }
        }
    }
}