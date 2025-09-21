using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.AI
{
    [CreateAssetMenu(fileName = "PvSoVictimSelection", menuName = "ProjectCI Tools/AI/Create VictimSelectionRegular")]
    public abstract class PvSoVictimSelectionRegular : PvSoVictimSelection
    {
        protected override int GetCellAggro(PvMnBattleGeneralUnit aiOwner, LevelCellBase cell)
        {
            var targetUnit = cell.GetUnitOnCell();
            if (targetUnit)
            {
                var attributes = targetUnit.RuntimeAttributes;
                var currentHealth = attributes.Health.CurrentValue;
            }

            return 0;
        }
    }
}
