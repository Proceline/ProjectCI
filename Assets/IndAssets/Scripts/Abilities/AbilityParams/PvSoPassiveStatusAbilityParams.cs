using System.Collections.Generic;
using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoPassiveStatusAbilityParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoPassiveStatusAbilityParams")]
    public class PvSoPassiveStatusAbilityParams : AbilityParamBase
    {
        [SerializeField] private PvSoPassiveStatus relatedStatus;

        [Header("Accuracy")] 
        [SerializeField] 
        private bool isAlwaysHitByDefault;

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase currentTargetCell, Queue<CommandResult> results, int passValue)
        {
            // ground status should always be hit
            var targetUnit = currentTargetCell.GetUnitOnCell();
            if (!targetUnit || targetUnit.IsDead())
            {
                return;
            }

            var isReallyHit = isAlwaysHitByDefault || passValue > 0;
            if (!isReallyHit) return;
            relatedStatus.InstallStatus(targetUnit);
            var statusCommand = new PvStatusApplyCommand
            {
                ResultId = resultId,
                AbilityId = ability.ID,
                OwnerId = fromUnit.ID,
                TargetCellIndex = targetUnit.GetCell().GetIndex(),
                StatusType = relatedStatus
            };

            results.Enqueue(statusCommand);
        }
    }
}
