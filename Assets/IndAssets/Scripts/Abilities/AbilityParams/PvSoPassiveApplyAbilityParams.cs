using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoPassiveApplyAbilityParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoPassiveApplyAbilityParams")]
    public class PvSoPassiveApplyAbilityParams : AbilityParamBase
    {
        [SerializeField] private PvSoPassiveBase applyPassive;

        [Header("Accuracy")] 
        [SerializeField]
        private bool isAlwaysHitByDefault;

        [SerializeField]
        private bool additionallyOnSelf;

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase currentTargetCell, Queue<CommandResult> results, int passValue, params uint[] extraInfos)
        {
            var targetUnit = currentTargetCell.GetUnitOnCell();
            if (additionallyOnSelf)
            {
                if (mainTarget != targetUnit)
                {
                    return;
                }

                targetUnit = fromUnit;
            }

            if (!targetUnit || targetUnit.IsDead())
            {
                return;
            }

            var isReallyHit = isAlwaysHitByDefault || passValue > 0;
            if (!isReallyHit) return;

            if (targetUnit is PvMnBattleGeneralUnit battleUnit)
            {
                applyPassive.InstallPassive(battleUnit);
                var statusCommand = new PvStatusApplyCommand
                {
                    ResultId = resultId,
                    OwnerId = fromUnit.ID,
                    TargetCellIndex = targetUnit.GetCell().GetIndex(),
                    StatusType = applyPassive.name
                };

                results.Enqueue(statusCommand);
            }
        }

        public override int MockValue(GridPawnUnit fromUnit, GridPawnUnit targetUnit, uint damageForm)
        {
            // Empty;
            return 0;
        }
    }
}
