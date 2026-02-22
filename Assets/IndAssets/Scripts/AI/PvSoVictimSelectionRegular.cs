using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace IndAssets.Scripts.AI
{
    [CreateAssetMenu(fileName = "PvSoVictimSelection", menuName = "ProjectCI Tools/AI/Create VictimSelectionRegular")]
    public class PvSoVictimSelectionRegular : PvSoVictimSelection
    {
        [SerializeField]
        private FeLiteGameRules modelRule;

        public const int AggroPointWithFatalToSelf = -90;
        public const int AggroPointWhileFatalDamage = 120;

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

        public override int GetCellAggro(PvMnBattleGeneralUnit aiOwner, LevelCellBase cell, PvSoUnitAbility ability)
        {
            var finalPoint = 0;
            var targetUnit = cell.GetUnitOnCell();
            if (targetUnit)
            {
                var mockResult = modelRule.MockAbilityToTargetCell(aiOwner, cell, ability);
                if (mockResult.TryGetValue(targetUnit, out var damage))
                {
                    var currentHealth = targetUnit.RuntimeAttributes.Health.CurrentValue;
                    currentHealth += damage;
                    if (currentHealth <= 0)
                    {
                        finalPoint += AggroPointWhileFatalDamage;
                    }
                    else
                    {
                        var minHealthCheck = 100 - currentHealth;
                        finalPoint += minHealthCheck;
                    }
                }

                if (mockResult.TryGetValue(aiOwner, out var damageOnSelf))
                {
                    var currentHealth = aiOwner.RuntimeAttributes.Health.CurrentValue;
                    currentHealth += damageOnSelf;
                    if (currentHealth <= 0)
                    {
                        finalPoint -= AggroPointWithFatalToSelf;
                    }
                    else
                    {
                        finalPoint += currentHealth;
                    }
                }
            }

            return finalPoint;
        }
    }
}
