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

        public override void Execute(string resultId,string abilityId, UnitAttributeContainer FromContainer, string FromUnitId,
            UnitAttributeContainer ToContainer, string ToUnitId, LevelCellBase ToCell, List<CommandResult> results)
        {
            int beforeHealth = ToContainer.Health.CurrentValue;
            int damage = FromContainer.GetAttributeValue(attackerAttribute);
            
            int deltaDamage = 
                Mathf.Max(damage - ToContainer.GetAttributeValue(defenderAttribute), 0);

            ToContainer.Health.ModifyValue(-deltaDamage);
            int afterHealth = ToContainer.Health.CurrentValue;
            
            if (results != null)
            {
                results.Add(new CommandDamageResult
                {
                    ResultId = resultId,
                    AbilityId = abilityId,
                    OwnerId = FromUnitId,
                    TargetCellIndex = ToCell.GetIndex(),
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
