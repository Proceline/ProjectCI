using System;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
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

        private int m_Index;

        [SerializeField]
        private Text m_DisplayNameText;

        public override string DisplayName
        {
            get => m_DisplayNameText.text; 
            set => m_DisplayNameText.text = value;
        }
        
        public override void SetAbility(UnitAbilityCore InAbility, int InIndex)
        {
            m_Toggle.isOn = false;
            _abilityData = InAbility;
            m_Index = InIndex;
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
            if(bIsOn)
            {
                if(_abilityData && Owner)
                {
                    Owner.HandleAbilitySelected(m_Index);
                }
            }
        }
    }
} 