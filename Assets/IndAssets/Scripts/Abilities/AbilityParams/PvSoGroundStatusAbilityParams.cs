using System.Collections.Generic;
using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoGroundStatusAbilityParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoGroundStatusAbilityParams")]
    public class PvSoGroundStatusAbilityParams : AbilityParamBase
    {
        [SerializeField] private PvSoGroundStatus relatedGroundStatus;
        
        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase targetCell, Queue<CommandResult> results, int passValue)
        {
            // ground status should always be hit
            relatedGroundStatus.AddGroundStatus(targetCell);
            var groundStatusCommand = new PvGroundStatusCommand
            {
                ResultId = resultId,
                AbilityId = ability.ID,
                OwnerId = fromUnit.ID,
                TargetCellIndex = targetCell.GetIndex(),
                RelatedGroundStatus = relatedGroundStatus
            };
            results.Enqueue(groundStatusCommand);
        }
    }
}
