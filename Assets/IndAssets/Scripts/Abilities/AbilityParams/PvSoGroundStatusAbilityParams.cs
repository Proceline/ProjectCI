using System.Collections.Generic;
using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoDirectDamageParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoDirectDamageParams")]
    public class PvSoGroundStatusAbilityParams : AbilityParamBase
    {
        [SerializeField] private PvSoGroundStatus relatedGroundStatus;
        
        // TODO: Refactor this into Interface
        [SerializeField]
        private PvSoTurnViewEndEvent onTurnAnimationEndEvent;

        public override string GetAbilityInfo()
        {
            // TODO: Description
            return base.GetAbilityInfo();
        }

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, Queue<CommandResult> results)
        {
            // TODO: effectedCells should be different
            List<LevelCellBase> effectedCells = ability.GetEffectedCells(fromUnit, toUnit.GetCell());

            foreach (var cell in effectedCells)
            {
                // ground status should always be hit
                relatedGroundStatus.AddGroundStatus(cell);
                // TODO: Directly apply turn end visual results
                // onTurnAnimationEndEvent
            }
        }
    }
}
