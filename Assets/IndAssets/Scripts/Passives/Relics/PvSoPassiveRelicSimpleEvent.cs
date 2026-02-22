using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;
using UnityEngine.Events;

namespace IndAssets.Scripts.Passives.Relics
{
    /// <summary>
    /// Only used for Attribute Modifier, thus, no general effect, only personal effect.
    /// The events are designed to be used for PvSoModifierSupportObject, 
    /// which will be added to the unit personally when the relic is settled, and removed when the relic is withdrawn.
    /// </summary>
    [CreateAssetMenu(fileName = "SoRelic_SimpleEventTBD", menuName = "ProjectCI Passives/Relics/SimpleEvent", order = 1)]
    public class PvSoPassiveRelicSimpleEvent : PvSoPassiveRelic
    {
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onRelicSettledForUnit;
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onRelicWithdrawFromUnit;
        
        protected override void InstallPassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            // Empty
        }

        protected override void DisposePassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            // Empty
        }

        protected override void InstallPassivePersonally(PvMnBattleGeneralUnit unit)
        {
            onRelicSettledForUnit.Invoke(unit);
        }

        protected override void DisposePassivePersonally(PvMnBattleGeneralUnit unit)
        {
            onRelicWithdrawFromUnit.Invoke(unit);
        }
    }
}