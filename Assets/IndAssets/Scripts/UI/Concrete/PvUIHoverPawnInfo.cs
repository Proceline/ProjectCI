using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvUIHoverPawnInfo : MonoBehaviour
    {
        [SerializeField] protected Text m_NameText;
        [SerializeField] private Text classText;
        [SerializeField] protected Text m_HitpointText;
        [SerializeField] private Slider currentValueSlider;
        [SerializeField] private Slider finalValueSlider;

        [SerializeField] protected AttributeType[] m_AttributesToDisplay;
        [SerializeField] protected Text[] m_AttributeValueTexts;

        [SerializeField] private Color[] teamColors;

        public void Setup(GridPawnUnit unit, int mockDelta)
        {
            if (unit)
            {
                SetupHealthMock(unit, mockDelta);

                for (int i = 0; i < m_AttributesToDisplay.Length; i++)
                {
                    m_AttributeValueTexts[i].text = unit.RuntimeAttributes.GetAttributeValue(m_AttributesToDisplay[i]).ToString();
                }
            }
        }

        private void SetupHealthMock(GridPawnUnit unit, int mockDelta)
        {
            var color = unit.GetTeam() == BattleTeam.Friendly ? teamColors[0] : teamColors[1];
            m_NameText.text = unit.GetUnitData().GetCharacterName();
            m_NameText.color = color;
            classText.text = unit.GetUnitData().GetClassName();
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
        }
    }
}