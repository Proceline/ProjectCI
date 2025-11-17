using System.Collections.Generic;
using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoGroundStatusAbilityParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoGroundStatusAbilityParams")]
    public class PvSoGroundStatusAbilityParams : AbilityParamBase
    {
        [SerializeField] private PvSoGroundStatus relatedGroundStatus;
        
        [SerializeField]
        private PvSoTurnViewEndEvent onTurnAnimationEndEvent;

        private readonly Queue<UnityAction> _pendingVisualActions = new();

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, Queue<CommandResult> results)
        {
            // TODO: effectedCells should be different
            List<LevelCellBase> effectedCells = ability.GetEffectedCells(fromUnit, toUnit.GetCell());

            foreach (var cell in effectedCells)
            {
                // ground status should always be hit
                relatedGroundStatus.AddGroundStatus(cell);
            }

            UnityAction groundVisualAction = () =>
            {
                relatedGroundStatus.RefreshVisualGroundStatus(effectedCells);
                while (_pendingVisualActions.TryDequeue(out var pendingAction))
                {
                    onTurnAnimationEndEvent.UnregisterCallback(pendingAction);
                }
            };
            _pendingVisualActions.Enqueue(groundVisualAction);
            onTurnAnimationEndEvent.RegisterCallback(groundVisualAction);
        }
    }
}
