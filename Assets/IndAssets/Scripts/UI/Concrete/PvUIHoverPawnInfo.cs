using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.Attributes;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvUIHoverPawnInfo : MonoBehaviour
    {
        [SerializeField] protected Text m_NameText;
        [SerializeField] protected Text m_HitpointText;
        [SerializeField] protected Slider m_HitpointSlider;

        [SerializeField] protected AttributeType m_PhysicalAttackAttribute;
        [SerializeField] protected AttributeType m_MagicAttackAttribute;
        [SerializeField] protected AttributeType[] m_AttributesToDisplay;
        [SerializeField] protected Text m_AttackValueText;
        [SerializeField] protected Text[] m_AttributeValueTexts;

        public void Setup(GridPawnUnit unit)
        {
            if (unit)
            {
                m_NameText.text = unit.GetUnitData().m_UnitName;
                m_HitpointText.text = unit.RuntimeAttributes.Health.CurrentValue.ToString()
                    + "/" + unit.RuntimeAttributes.Health.MaxValue.ToString();
                m_HitpointSlider.maxValue = unit.RuntimeAttributes.Health.MaxValue;
                m_HitpointSlider.value = unit.RuntimeAttributes.Health.CurrentValue;

                m_AttackValueText.text = unit.RuntimeAttributes.GetAttributeValue(m_PhysicalAttackAttribute).ToString();
                for (int i = 0; i < m_AttributesToDisplay.Length; i++)
                {
                    m_AttributeValueTexts[i].text = unit.RuntimeAttributes.GetAttributeValue(m_AttributesToDisplay[i]).ToString();
                }
            }
        }
    }
}