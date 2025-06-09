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
        enum EAbilityListState
        {
            None,
            AbilitiesEnabled,
            AbilityTargeting
        }

        [NonSerialized]
        private ToggleGroup m_ToggleGroup;

        private Stack<EAbilityListState> _statesStack = new Stack<EAbilityListState>();
        internal static Action<AbilityListUIElementBase, UnitAbilityCore> OnAbilitySelected;

        public override void InitializeUI(Camera InUICamera)
        {
            base.InitializeUI(InUICamera);

            if (!m_ToggleGroup)
            {
                m_ToggleGroup = gameObject.AddComponent<ToggleGroup>();
                m_ToggleGroup.allowSwitchOff = false;

            }
        }

        private EAbilityListState GetCurrentState()
        {
            if (_statesStack.Count > 0)
            {
                return _statesStack.Peek();
            }
            return EAbilityListState.None;
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
                OnAbilitySelected += OnAbilityTargetingStarted;
            }
            else
            {
                _statesStack.Clear();
                m_SelectedUnit.UnBindFromOnMovementPostCompleted(ShowAbilitiesPostMovement);
                OnAbilitySelected -= OnAbilityTargetingStarted;
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
            if (GetCurrentState() == EAbilityListState.None)
            {
                _statesStack.Push(EAbilityListState.AbilitiesEnabled);
                RespondToStateChange(GetCurrentState());
            }
        }

        private void OnAbilityTargetingStarted(AbilityListUIElementBase abilityListUIElementBase, UnitAbilityCore unitAbilityCore)
        {
            if (abilityListUIElementBase != this)
            {
                return;
            }
            _statesStack.Push(EAbilityListState.AbilityTargeting);
            RespondToStateChange(GetCurrentState());
        }

        private void RespondToStateChange(EAbilityListState state)
        {
            switch (state)
            {
                case EAbilityListState.AbilitiesEnabled:
                    uiContainer.transform.position = m_SelectedUnit.transform.position;
                    uiContainer.gameObject.SetActive(true);
                    break;
                case EAbilityListState.AbilityTargeting:
                    uiContainer.gameObject.SetActive(false);
                    break;
            }
        }
    }
}
