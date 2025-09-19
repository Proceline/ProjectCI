using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.Utilities.Runtime.Events;
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
        
        private readonly ServiceLocator<PvSoUnitSelectEvent> _selectEventLocator = new();

        [NonSerialized] 
        private GridPawnUnit _currentUnit;

        public bool bIsSelectedEnabled;

        private bool _bEnabled = true;
        private bool _bInitialized;

        public void Initialize()
        {
            if (_bInitialized)
                return;

            _bInitialized = true;
            SetupScreen();
            
            // TODO: Same todo as below
            // TacticBattleManager battleManager = TacticBattleManager.Get();
            // if (battleManager)
            // {
            //     battleManager.OnUnitHover.AddListener(HandleUnitHover);
            //     battleManager.OnTeamWon.AddListener(HandleGameDone);
            // }
            
            if (bIsSelectedEnabled)
            {
                _selectEventLocator.Service.RegisterCallback(HandleUnitSelected);
            }
        }

        private void OnDestroy()
        {
            if (_bInitialized)
            {
                if (bIsSelectedEnabled)
                {
                    _selectEventLocator.Service.UnregisterCallback(HandleUnitSelected);
                }
            }
        }

        private void HandleUnitSelected(IEventOwner owner, UnitSelectEventParam selectInfo)
        {
            HandleUnitSelected(selectInfo.Behaviour == UnitSelectBehaviour.Select ? selectInfo.Unit : null);
        }
        
        private void HandleUnitSelected(GridPawnUnit inUnit)
        {
            // TODO: Handle Hover
            // TacticBattleManager battleManager = TacticBattleManager.Get();
            if (inUnit)
            {
                HandleUnitHover(inUnit);
                // battleManager.OnUnitHover.RemoveListener(HandleUnitHover);
            }
            else
            {
                m_ScreenObject.SetActive(false);
                // battleManager.OnUnitHover.AddListener(HandleUnitHover);
            }

            m_OnUnitSelected.Invoke(inUnit);
        }

        public void OnEnableExtraUIHoverPanel(GridPawnUnit InUnit)
        {
            if (InUnit)
            {
                _bEnabled = true;
                m_ScreenObject.SetActive(true);
            }
            else
            {
                _bEnabled = false;
                m_ScreenObject.SetActive(false);
            }
        }

        protected virtual void HandleUnitHover(GridPawnUnit InUnit)
        {
            _currentUnit = InUnit;
            SetupScreen();
        }

        protected void SetupScreen()
        {
            if (_currentUnit && _bEnabled)
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
            if (_currentUnit)
            {
                m_NameText.text = _currentUnit.GetUnitData().m_UnitName;
                m_HitpointText.text = _currentUnit.RuntimeAttributes.Health.CurrentValue.ToString() 
                    + "/" + _currentUnit.RuntimeAttributes.Health.MaxValue.ToString();
                m_HitpointSlider.maxValue = _currentUnit.RuntimeAttributes.Health.MaxValue;
                m_HitpointSlider.value = _currentUnit.RuntimeAttributes.Health.CurrentValue;

                m_AttackValueText.text = _currentUnit.RuntimeAttributes.GetAttributeValue(m_PhysicalAttackAttribute).ToString();
                for (int i = 0; i < m_AttributesToDisplay.Length; i++)
                {
                    m_AttributeValueTexts[i].text = _currentUnit.RuntimeAttributes.GetAttributeValue(m_AttributesToDisplay[i]).ToString();
                }
            }
        }
    }
}