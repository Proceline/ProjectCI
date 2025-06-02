using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.Attributes;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvUIHoverPawnInfo : MonoBehaviour
    {
        public GameObject m_ScreenObject;

        [SerializeField] private Image m_AttackTypeIcon;
        [SerializeField] private Sprite m_PhysicalSprite;
        [SerializeField] private Sprite m_MagicSprite;

        [SerializeField] private Text m_NameText;
        [SerializeField] private Text m_HitpointText;
        [SerializeField] private Slider m_HitpointSlider;

        [SerializeField] private AttributeType m_PhysicalAttackAttribute;
        [SerializeField] private AttributeType m_MagicAttackAttribute;
        [SerializeField] private AttributeType[] m_AttributesToDisplay;
        [SerializeField] private Text m_AttackValueText;
        [SerializeField] private Text[] m_AttributeValueTexts;

        protected GridPawnUnit m_CurrUnit = null;
        private bool bEnabled = true;
        private bool _bInitialized = false;

        public void Initialize()
        {
            if (_bInitialized)
                return;

            _bInitialized = true;
            SetupScreen();
            TacticBattleManager.Get().OnUnitHover.AddListener(HandleUnitHover);
            TacticBattleManager.Get().OnTeamWon.AddListener(HandleGameDone);
        }

        private void OnDestroy()
        {
            if (_bInitialized)
            {
                TacticBattleManager.Get().OnUnitHover.RemoveListener(HandleUnitHover);
                TacticBattleManager.Get().OnTeamWon.RemoveListener(HandleGameDone);
            }
        }

        protected virtual void HandleUnitHover(GridPawnUnit InUnit)
        {
            if (m_CurrUnit)
            {
                BattleHealth hpComp = m_CurrUnit.GetComponent<BattleHealth>();
                if (hpComp)
                {
                    hpComp.OnHealthPreDepleted.RemoveListener(HandleUnitDeath);
                }
            }

            m_CurrUnit = InUnit;

            if (m_CurrUnit)
            {
                BattleHealth hpComp = m_CurrUnit.GetComponent<BattleHealth>();
                if (hpComp)
                {
                    hpComp.OnHealthPreDepleted.AddListener(HandleUnitDeath);
                }
            }

            SetupScreen();
        }

        protected virtual void HandleUnitDeath()
        {
            HandleUnitHover(null);
        }

        protected void SetupScreen()
        {
            if (m_CurrUnit && bEnabled)
            {
                m_ScreenObject.SetActive(true);
                UpdateViewInfo();
            }
            else
            {
                m_ScreenObject.SetActive(false);
            }
        }

        protected virtual void UpdateViewInfo()
        {
            if (m_CurrUnit)
            {
                m_NameText.text = m_CurrUnit.GetUnitData().m_UnitName;
                m_HitpointText.text = m_CurrUnit.RuntimeAttributes.Health.CurrentValue.ToString() 
                    + "/" + m_CurrUnit.RuntimeAttributes.Health.MaxValue.ToString();
                m_HitpointSlider.maxValue = m_CurrUnit.RuntimeAttributes.Health.MaxValue;
                m_HitpointSlider.value = m_CurrUnit.RuntimeAttributes.Health.CurrentValue;

                m_AttackValueText.text = m_CurrUnit.RuntimeAttributes.GetAttributeValue(m_PhysicalAttackAttribute).ToString();
                for (int i = 0; i < m_AttributesToDisplay.Length; i++)
                {
                    m_AttributeValueTexts[i].text = m_CurrUnit.RuntimeAttributes.GetAttributeValue(m_AttributesToDisplay[i]).ToString();
                }
            }
        }

        void HandleGameDone(BattleTeam InWinningTeam)
        {
            m_ScreenObject.SetActive(false);
            bEnabled = false;
        }
    }
}