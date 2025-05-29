using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [CreateAssetMenu(fileName = "PvSoDirectDamageParams", menuName = "ProjectCI Tools/Ability/Parameters/PvSoDirectDamageParams")]
    public class PvSoDirectDamageAbilityParams : AbilityParamBase
    {
        public int m_Damage;
        public bool m_bMagicalDamage;

        public AttributeType attackerAttribute;
        public AttributeType defenderAttribute;

        public AttributeType damageType;

        public override string GetAbilityInfo()
        {
            return "Damage" + (m_bMagicalDamage ? "(Magical)" : "") + " " + m_Damage.ToString();
        }

        public override void Execute(string abilityId, UnitAttributeContainer FromContainer, string FromUnitId,
            UnitAttributeContainer ToContainer, string ToUnitId, LevelCellBase ToCell, List<CommandResult> results)
        {
            int beforeHealth = ToContainer.Health.CurrentValue;
            int damage = m_Damage;
            if (m_bMagicalDamage)
            {
                damage = FromContainer.GetAttributeValue(attackerAttribute);
            }
            
            int deltaDamage = damage;

            ToContainer.Health.ModifyValue(-deltaDamage);
            int afterHealth = ToContainer.Health.CurrentValue;
            
            if (results != null)
            {
                results.Add(new CommandDamageResult
                {
                    ResultId = abilityId,
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
