using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Commands.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "PvSoPushParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoPushParams")]
    public class PvSoPushAbilityParams : AbilityParamBase
    {
        [SerializeField] private int pushDistance;
        
        [Header("Accuracy")] 
        [SerializeField] 
        private bool isAlwaysHitByDefault;
        [SerializeField] private AttributeType hitAttribute;
        [SerializeField] private AttributeType dodgeAttribute;

        public override string GetAbilityInfo()
        {
            // TODO: Description
            return base.GetAbilityInfo();
        }

        public override void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, Queue<CommandResult> results)
        {
            if (toUnit.IsDead())
            {
                return;
            }
            
            var toContainer = toUnit.RuntimeAttributes;
            var fromContainer = fromUnit.RuntimeAttributes;

            bool isReallyHit = isAlwaysHitByDefault;
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

            if (!isReallyHit)
            {
                return;
            }
            
            var pushCommand = new PvPushCommand
            {
                ResultId = resultId,
                AbilityId = ability.ID,
                OwnerId = fromUnit.ID,
                TargetCellIndex = toUnit.GetCell().GetIndex(),
                CommandType = "ForceMove",
                Value = pushDistance,
                FromCell = fromUnit.GetCell()
            };

            results.Enqueue(pushCommand);
        }
    }
}
