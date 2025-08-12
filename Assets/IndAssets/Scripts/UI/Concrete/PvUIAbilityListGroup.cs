using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI;
using ProjectCI.Utilities.Runtime.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    /// <summary>
    /// Add it to SpawnOnStart in TacticBattleManager
    /// </summary>
    public class PvUIAbilityListGroup : AbilityListUIElementBase
    {
        [NonSerialized]
        private ToggleGroup m_ToggleGroup;
        private readonly ServiceLocator<PvSoUnitBattleStateEvent> _stateEventLocator = new();
        private readonly ServiceLocator<PvSoUnitSelectEvent> _selectEventLocator = new();

        public override void InitializeUI()
        {
            base.InitializeUI();
            _selectEventLocator.Service.RegisterCallback(HandleUnitSelected);

            
            if (!m_ToggleGroup)
            {
                m_ToggleGroup = gameObject.AddComponent<ToggleGroup>();
                m_ToggleGroup.allowSwitchOff = false;

            }
            
            _stateEventLocator.Service.RegisterCallback(RespondToSelectedUnitState);
        }

        protected void OnDestroy()
        {
            _stateEventLocator.Service.UnregisterCallback(RespondToSelectedUnitState);
            _selectEventLocator.Service.UnregisterCallback(HandleUnitSelected);
        }

        private void HandleUnitSelected(IEventOwner owner, UnitSelectEventParam selectInfo)
        {
            
        }

        protected override void HandleUnitPostSelected(bool bIsSelected) =>
            uiContainer.gameObject.SetActive(bIsSelected);
        

        protected override void SetupAbilitySlot(SlotDataUIElementBase slot, 
            UnitAbilityCore ability, int InIndex)
        {
            base.SetupAbilitySlot(slot, ability, InIndex);

            var toggle = slot.GetComponent<Toggle>();
            toggle.group = m_ToggleGroup;
            m_ToggleGroup.RegisterToggle(toggle);
        }

        protected override void ClearAbilitySlot(SlotDataUIElementBase slot)
        {
            base.ClearAbilitySlot(slot);

            var toggle = slot.GetComponent<Toggle>();
            m_ToggleGroup.UnregisterToggle(toggle);
        }

        private void RespondToSelectedUnitState(IEventOwner eventOwner, UnitStateEventParam parameter)
        {
            if (!SelectedUnit)
            {
                return;
            }
            
            if (SelectedUnit.ID == eventOwner.EventIdentifier)
            {
                if (parameter is
                    {
                        battleState: UnitBattleState.UsingAbility,
                        behaviour: UnitStateBehaviour.Adding or UnitStateBehaviour.Emphasis
                    })
                {
                    uiContainer.transform.position = SelectedUnit.transform.position;
                    uiContainer.gameObject.SetActive(true);
                }
                else
                {
                    uiContainer.gameObject.SetActive(false);
                }
            }
        }
    }
}
