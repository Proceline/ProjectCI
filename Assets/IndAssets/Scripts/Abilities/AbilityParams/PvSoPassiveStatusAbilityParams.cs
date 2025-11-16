using System.Collections.Generic;
using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.Runtime.Abilities.Projectiles;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoPassiveStatusAbilityParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoPassiveStatusAbilityParams")]
    public class PvSoPassiveStatusAbilityParams : AbilityParamBase
    {
        [SerializeField] private PvSoPassiveStatus relatedStatus;

        [Header("Accuracy")] 
        [SerializeField] 
        private bool isAlwaysHitByDefault;
        [SerializeField] private AttributeType hitAttribute;
        [SerializeField] private AttributeType dodgeAttribute;

        private readonly Queue<UnityAction> _pendingVisualActions = new();

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, Queue<CommandResult> results)
        {
            // TODO: effectedCells should be different
            List<LevelCellBase> effectedCells = ability.GetEffectedCells(fromUnit, toUnit.GetCell());
            var fromContainer = fromUnit.RuntimeAttributes;

            foreach (var cell in effectedCells)
            {
                // ground status should always be hit
                var targetUnit = cell.GetUnitOnCell();
                if (!targetUnit)
                {
                    continue;
                }
                
                if (targetUnit.IsDead())
                {
                    continue;
                }

                var toContainer = targetUnit.RuntimeAttributes;
                var isReallyHit = isAlwaysHitByDefault;
                
                if (!isAlwaysHitByDefault)
                {
                    var hitThreshold = fromContainer.GetAttributeValue(hitAttribute);
                    var dodgeThreshold = toContainer.GetAttributeValue(dodgeAttribute);
                    var hitPercentageResult = hitThreshold - dodgeThreshold;
                    if (hitPercentageResult >= 100)
                    {
                        isReallyHit = true;
                    }
                    else if (hitPercentageResult <= 0)
                    {
                        isReallyHit = false;
                    }
                    else
                    {
                        var randomValue = Random.Range(0, 10000) % 100;
                        isReallyHit = randomValue < hitPercentageResult;
                    }
                }

                if (!isReallyHit) continue;
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
}
