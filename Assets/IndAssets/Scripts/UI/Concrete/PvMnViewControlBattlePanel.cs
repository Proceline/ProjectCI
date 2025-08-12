using System;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine.UI;
using ProjectCI.Runtime.Utilities;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.Runtime.GUI.Battle
{
    /// <summary>
    /// Add it to SpawnOnStart in TacticBattleManager
    /// </summary>
    public class PvMnViewControlBattlePanel : MonoBehaviour
    {
        [NonSerialized]
        private ToggleGroup _toggleGroup;

        [NonSerialized] 
        private PvMnBattleGeneralUnit _ownerPlayerUnit;
        
        [SerializeField] 
        private GameObject containerView;
        
        [SerializeField] 
        private Transform abilitiesContainerView;

        [SerializeField] 
        private bool visibleAtBeginning;

        [SerializeField]
        private bool worldSpaceView;
        
        [SerializeField]
        private Vector3 worldSpaceOffset = Vector3.zero;

        [SerializeField]
        private PvMnViewAbilityBattleSlot slotPrefab;
        
        private readonly ServiceLocator<PvSoUnitBattleStateEvent> _stateEventLocator = new();
        private readonly ServiceLocator<PvSoUnitSelectEvent> _selectEventLocator = new();

        private readonly PvSimpleWidgetsContainer<PvMnViewAbilityBattleSlot> _battleSlotsContainer = new();

        public void Setup()
        {
            _selectEventLocator.Service.RegisterCallback(HandleUnitSelected);
            if (!_toggleGroup)
            {
                _toggleGroup = gameObject.AddComponent<ToggleGroup>();
                _toggleGroup.allowSwitchOff = false;
            }

            _stateEventLocator.Service.RegisterCallback(RespondToSelectedUnitState);
            containerView.SetActive(visibleAtBeginning);

            if (!worldSpaceView) return;
            var uiCamera = Array.Find(Camera.allCameras, camObj => camObj.CompareTag("UICamera"));
            if (uiCamera)
            {
                transform.rotation = uiCamera.transform.rotation;
            }
        }

        protected void OnDestroy()
        {
            _stateEventLocator.Service.UnregisterCallback(RespondToSelectedUnitState);
            _selectEventLocator.Service.UnregisterCallback(HandleUnitSelected);
        }

        private void HandleUnitSelected(IEventOwner owner, UnitSelectEventParam selectInfo)
        {
            var inputUnit = selectInfo.Unit;
            if (inputUnit)
            {
                _ownerPlayerUnit = inputUnit;
                SetupAbilityList();
                HandleUnitPostSelected(true);
            }
            else
            {
                ClearAbilityList();
                HandleUnitPostSelected(false);
                _ownerPlayerUnit = null;
            }
        }

        private void HandleUnitPostSelected(bool bIsSelected)
        {
            containerView.gameObject.SetActive(bIsSelected);
        }

        private void RespondToSelectedUnitState(IEventOwner eventOwner, UnitStateEventParam parameter)
        {
            if (!_ownerPlayerUnit)
            {
                return;
            }

            if (_ownerPlayerUnit.ID != eventOwner.EventIdentifier) return;
            if (parameter is
                {
                    battleState: UnitBattleState.UsingAbility,
                    behaviour: UnitStateBehaviour.Adding or UnitStateBehaviour.Emphasis
                })
            {
                if (worldSpaceView)
                {
                    containerView.transform.position = _ownerPlayerUnit.transform.position + worldSpaceOffset;
                }

                containerView.gameObject.SetActive(true);
            }
            else
            {
                containerView.gameObject.SetActive(false);
            }
        }

        // TODO: Ability List refactor to FE style
        private void SetupAbilityList()
        {
            if (!_ownerPlayerUnit)
            {
                return;
            }

            var abilities = _ownerPlayerUnit.GetUsableAbilities();
            int requiredCount = abilities.Count;

            _battleSlotsContainer.TrimOrExtendCollection(slotPrefab, abilitiesContainerView, requiredCount);

            for (int i = 0; i < requiredCount; i++)
            {
                var slot = _battleSlotsContainer.GetWidget(i);
                var ability = abilities[i];

                if (ability)
                {
                    slot.gameObject.SetActive(true);
                    slot.Setup(ability);

                    var toggle = slot.toggle;
                    toggle.group = _toggleGroup;
                    _toggleGroup.RegisterToggle(toggle);
                    
                    // TODO: Handle default selection
                    // slot.SwitchWithoutNotify();.ForceHighlight(ability == SelectedUnit.GetCurrentUnitAbility());
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }

        private void ClearAbilityList()
        {
            _battleSlotsContainer.DoSomethingForEach(slot =>
            {
                slot.CleanUp();
                slot.gameObject.SetActive(false);
                _toggleGroup.UnregisterToggle(slot.toggle);
            });
        }
    }
}
