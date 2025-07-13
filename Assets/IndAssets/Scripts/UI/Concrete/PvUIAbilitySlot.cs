using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public class PvUIAbilitySlot : SlotDataUIElementBase
    {
        [SerializeField]
        private Toggle m_Toggle;

        [NonSerialized]
        private UnitAbilityCore _abilityData;

        [SerializeField]
        private Text m_DisplayNameText;

        [SerializeField] 
        private Color[] selectedColors;

        [SerializeField] 
        private PvSoAbilitySelectEvent abilitySelectEvent;

        public override string DisplayName
        {
            get => m_DisplayNameText.text; 
            set => m_DisplayNameText.text = value;
        }
        
        public override void SetAbility(UnitAbilityCore InAbility, int InIndex)
        {
            m_Toggle.isOn = false;
            _abilityData = InAbility;
            SetDisplayName();
            CheckAvailability();
            m_Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        public override void ClearAbility()
        {
            base.ClearAbility();
            m_Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void CheckAvailability()
        {
            if(_abilityData && Owner)
            {
                // var resourceComponent = Owner.GetComponent<PvMnBattleResourceContainer>();
                // int currentStamina = resourceComponent.GetStamina();
                // if(currentStamina >= _abilityData.StaminaCost)
                // {
                //     // GetComponent<Toggle>().interactable = true;
                // }
                // else
                // {
                //     // GetComponent<Toggle>().interactable = false;
                // }
            }
        }

        private void SetDisplayName()
        {
            DisplayName = _abilityData.GetAbilityName();
        }

        public override void OnHover()
        {
            base.OnHover();
        }

        private void OnToggleValueChanged(bool bIsOn)
        {
            if (bIsOn)
            {
                if (_abilityData && Owner)
                {
                    abilitySelectEvent.Raise(_abilityData);
                }
            }

            m_DisplayNameText.color = bIsOn ? selectedColors[1] : selectedColors[0];
        }

        protected internal override void ForceHighlight(bool enable)
        {
            if (enable)
            {
                m_Toggle.SetIsOnWithoutNotify(true);
                m_DisplayNameText.color = selectedColors[1];
            }
        }
    }
} 