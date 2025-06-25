using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "PvSoDirectDamageParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoDirectDamageParams")]
    public class PvSoDirectDamageAbilityParams : AbilityParamBase
    {
        // TODO: This will affect player's attack might
        public int m_Damage;

        public AttributeType attackerAttribute;
        public AttributeType defenderAttribute;

        public override string GetAbilityInfo()
        {
            // TODO: Description
            return base.GetAbilityInfo();
        }

        public override void Execute(string resultId,string abilityId, UnitAttributeContainer fromContainer, string fromUnitId,
            UnitAttributeContainer toContainer, string toUnitId, LevelCellBase toCell, List<CommandResult> results)
        {
            int beforeHealth = toContainer.Health.CurrentValue;
            int damage = fromContainer.GetAttributeValue(attackerAttribute);
            
            int deltaDamage = 
                Mathf.Max(damage - toContainer.GetAttributeValue(defenderAttribute), 0);

            toContainer.Health.ModifyValue(-deltaDamage);
            int afterHealth = toContainer.Health.CurrentValue;
            
            if (results != null)
            {
                results.Add(new CommandDamageResult
                {
                    ResultId = resultId,
                    AbilityId = abilityId,
                    OwnerId = fromUnitId,
                    TargetCellIndex = toCell.GetIndex(),
                    BeforeValue = beforeHealth,
                    AfterValue = afterHealth,
                    CommandType = CommandDamageResult.TakeDamage,
                    Value = deltaDamage,
                    ExtraInfo = nameof(UnitAttributeContainer.Health)
                });
            }
        }
    }
}
