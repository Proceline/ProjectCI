using System;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnViewAbilityBattleSlot : MonoBehaviour
    {
        [SerializeField]
        internal Toggle toggle;

        [NonSerialized]
        private PvSoUnitAbility _abilityData;

        [FormerlySerializedAs("abilitySelectEvent")] [SerializeField] 
        private PvSoAbilitySelectEvent raiserAbilitySelectEvent;
        
        public void Setup(PvSoUnitAbility ability)
        {
            toggle.isOn = false;
            _abilityData = ability;
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        public void CleanUp()
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool bIsOn)
        {
            if (bIsOn && _abilityData)
            {
                raiserAbilitySelectEvent.Raise(_abilityData);
            }
        }

        public void SwitchWithoutNotify(bool isEnabled) => toggle.SetIsOnWithoutNotify(isEnabled);
    }
} 