using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.Attributes;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvUIHoverPawnInfo : MonoBehaviour
    {
        public GameObject m_ScreenObject;

        [SerializeField] protected Image m_AttackTypeIcon;
        [SerializeField] protected Sprite m_PhysicalSprite;
        [SerializeField] protected Sprite m_MagicSprite;

        [SerializeField] protected Text m_NameText;
        [SerializeField] protected Text m_HitpointText;
        [SerializeField] protected Slider m_HitpointSlider;

        [SerializeField] protected AttributeType m_PhysicalAttackAttribute;
        [SerializeField] protected AttributeType m_MagicAttackAttribute;
        [SerializeField] protected AttributeType[] m_AttributesToDisplay;
        [SerializeField] protected Text m_AttackValueText;
        [SerializeField] protected Text[] m_AttributeValueTexts;
        [SerializeField] protected UnityEvent<GridPawnUnit> m_OnUnitSelected;

        protected GridPawnUnit m_CurrUnit = null;
        public bool bIsSelectedEnabled = false;

        private bool bEnabled = true;
        private bool _bInitialized = false;

        public void Initialize()
        {
            if (_bInitialized)
                return;

            _bInitialized = true;
            SetupScreen();
            TacticBattleManager battleManager = TacticBattleManager.Get();
            if (battleManager)
            {
                battleManager.OnUnitHover.AddListener(HandleUnitHover);
                if (bIsSelectedEnabled)
                {
                    battleManager.OnUnitSelected.AddListener(HandleUnitSelected);
                }
                battleManager.OnTeamWon.AddListener(HandleGameDone);
            }
        }

        private void OnDestroy()
        {
            if (_bInitialized)
            {
                TacticBattleManager battleManager = TacticBattleManager.Get();
                if (battleManager)
                {
                    battleManager.OnUnitHover.RemoveListener(HandleUnitHover);
                    if (bIsSelectedEnabled)
                    {
                        battleManager.OnUnitSelected.RemoveListener(HandleUnitSelected);
                    }
                    battleManager.OnTeamWon.RemoveListener(HandleGameDone);
                }
            }
        }

        protected void HandleUnitSelected(GridPawnUnit InUnit)
        {
            TacticBattleManager battleManager = TacticBattleManager.Get();
            if (InUnit)
            {
                HandleUnitHover(InUnit);
                battleManager.OnUnitHover.RemoveListener(HandleUnitHover);
            }
            else
            {
                m_ScreenObject.SetActive(false);
                battleManager.OnUnitHover.AddListener(HandleUnitHover);
            }

            m_OnUnitSelected.Invoke(InUnit);
        }

        public void OnEnableExtraUIHoverPanel(GridPawnUnit InUnit)
        {
            if (InUnit)
            {
                bEnabled = true;
                m_ScreenObject.SetActive(true);
            }
            else
            {
                bEnabled = false;
                m_ScreenObject.SetActive(false);
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