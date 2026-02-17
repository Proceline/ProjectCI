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
        [SerializeField] private Slider currentValueSlider;
        [SerializeField] private Slider finalValueSlider;

        [SerializeField] protected AttributeType m_PhysicalAttackAttribute;
        [SerializeField] protected AttributeType m_MagicAttackAttribute;
        [SerializeField] protected AttributeType[] m_AttributesToDisplay;
        [SerializeField] protected Text m_AttackValueText;
        [SerializeField] protected Text[] m_AttributeValueTexts;

        public void Setup(GridPawnUnit unit, int mockDelta)
        {
            if (unit)
            {
                m_NameText.text = unit.GetUnitData().m_UnitName;
                var maxHealth = unit.RuntimeAttributes.Health.MaxValue;
                var currHealth = unit.RuntimeAttributes.Health.CurrentValue;
                var mockHealth = currHealth + mockDelta;

                if (mockDelta == 0)
                {
                    m_HitpointText.text = $"{currHealth.ToString()} / {maxHealth.ToString()}";
                }
                else
                {
                    var additionalInfo = mockDelta > 0 ? $"<color=yellow>+{mockDelta}</color>" : $"<color=red>{mockDelta}</color>";
                    m_HitpointText.text = $"{currHealth.ToString()} {additionalInfo} / {maxHealth.ToString()}";
                }
                    
                finalValueSlider.maxValue = maxHealth;
                finalValueSlider.value = mockHealth;

                if (mockDelta != 0)
                {
                    currentValueSlider.gameObject.SetActive(true);
                    currentValueSlider.fillRect.gameObject.SetActive(true);
                    currentValueSlider.maxValue = maxHealth;
                    currentValueSlider.value = currHealth;
                }
                else
                {
                    currentValueSlider.gameObject.SetActive(false);
                    currentValueSlider.fillRect.gameObject.SetActive(false);
                }
                
                m_AttackValueText.text = unit.RuntimeAttributes.GetAttributeValue(m_PhysicalAttackAttribute).ToString();
                for (int i = 0; i < m_AttributesToDisplay.Length; i++)
                {
                    m_AttributeValueTexts[i].text = unit.RuntimeAttributes.GetAttributeValue(m_AttributesToDisplay[i]).ToString();
                }
            }
        }
    }
}