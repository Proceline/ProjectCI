using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.UI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    /// <summary>
    /// Add it to SpawnOnStart in TacticBattleManager
    /// </summary>
    public class PvUIAbilityListGroup : AbilityListUIElementBase
    {
        [NonSerialized]
        private ToggleGroup m_ToggleGroup;

        public override void InitializeUI(Camera InUICamera)
        {
            base.InitializeUI(InUICamera);

            if (!m_ToggleGroup)
            {
                m_ToggleGroup = gameObject.AddComponent<ToggleGroup>();
                m_ToggleGroup.allowSwitchOff = false;

            }
        }

        protected override void HandleUnitPostSelected(bool bIsSelected)
        {
            if (!m_SelectedUnit)
            {
                return;
            }
            
            uiContainer.gameObject.SetActive(false);
            if (bIsSelected)
            {
                m_SelectedUnit.BindToOnMovementPostCompleted(ShowAbilitiesPostMovement);
            }
            else
            {
                m_SelectedUnit.UnBindFromOnMovementPostCompleted(ShowAbilitiesPostMovement);
            }
        }

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

        private void ShowAbilitiesPostMovement()
        {
            uiContainer.transform.position = m_SelectedUnit.transform.position;
            uiContainer.gameObject.SetActive(true);
        }
    }
}
