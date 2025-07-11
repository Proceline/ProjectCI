using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI
{
    /// <summary>
    /// Add it to SpawnOnStart in TacticBattleManager
    /// </summary>
    public class AbilityListUIElementBase : MonoBehaviour
    {
        [NonSerialized]
        protected GridPawnUnit SelectedUnit;

        [SerializeField]
        protected GameObject uiContainer;

        [SerializeField]
        private Transform listContainer;

        [SerializeField]
        private SlotDataUIElementBase abilityUIElementPrefab;

        private readonly List<SlotDataUIElementBase> _uiAbilities = new();

        public virtual void InitializeUI()
        {
            ClearAbilityList();
            uiContainer.gameObject.SetActive(false);
        }

        protected void HandleUnitSelected(GridPawnUnit InUnit)
        {
            if(InUnit)
            {
                SelectedUnit = InUnit;
                SetupAbilityList();
                HandleUnitPostSelected(true);
            }
            else
            {
                ClearAbilityList();
                HandleUnitPostSelected(false);
                SelectedUnit = null;
            }
        }

        protected virtual void HandleUnitPostSelected(bool bIsSelected)
        {
            // Do nothing
        }

        private void SetupAbilityList()
        {
            if (!SelectedUnit)
            {
                return;
            }

            var abilities = SelectedUnit.GetAbilities();
            int requiredCount = abilities.Count;

            while (_uiAbilities.Count < requiredCount)
            {
                var newAbilityUI = Instantiate(abilityUIElementPrefab, listContainer);
                _uiAbilities.Add(newAbilityUI);
            }

            for (int i = 0; i < requiredCount; i++)
            {
                var abilityUI = _uiAbilities[i];
                var ability = abilities[i];

                if (ability)
                {
                    SetupAbilitySlot(abilityUI, ability, i);
                    abilityUI.ForceHighlight(ability == SelectedUnit.GetCurrentAbility());
                }
                else
                {
                    abilityUI.gameObject.SetActive(false);
                }
            }

            for (int i = requiredCount; i < _uiAbilities.Count; i++)
            {
                _uiAbilities[i].gameObject.SetActive(false);
            }
        }

        protected virtual void SetupAbilitySlot(SlotDataUIElementBase slot, 
            UnitAbilityCore ability, int InIndex)
        {
            slot.gameObject.SetActive(true);
            slot.SetOwner(this);
            slot.SetAbility(ability, InIndex);
        }

        private void ClearAbilityList()
        {
            foreach (SlotDataUIElementBase abilityUI in _uiAbilities)
            {
                if (abilityUI)
                {
                    ClearAbilitySlot(abilityUI);
                }
            }
        }

        protected virtual void ClearAbilitySlot(SlotDataUIElementBase slot)
        {
            slot.SetOwner(this);
            slot.ClearAbility();
            slot.gameObject.SetActive(false);
        }
    }
}
