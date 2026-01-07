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
        
        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            onRelicSettledForUnit.Invoke(unit);
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            onRelicWithdrawFromUnit.Invoke(unit);
        }
    }
}