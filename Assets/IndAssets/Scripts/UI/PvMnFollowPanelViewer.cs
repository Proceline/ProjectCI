using System;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnFollowPanelViewer : MonoBehaviour
    {
        [SerializeField] 
        private PvSoUnitBattleStateEvent onStateDetermined;

        [SerializeField] 
        private Camera controlUiCamera;
        
        /// <summary>
        /// Root of Canvas
        /// </summary>
        [SerializeField] 
        private GameObject canvasGameObject;

        /// <summary>
        /// Root of Attack/Ride/Support/Wait
        /// </summary>
        [SerializeField]
        private PvMnControlPanel mainControlPanel;

        [SerializeField]
        private PvMnControlPanelDynamic sideControlPanel;

        [NonSerialized]
        private PvMnBattleGeneralUnit _determinedUnit;

        /// <summary>
        /// Disable BattleActionCancel input action will be bind in here
        /// </summary>
        [SerializeField]
        private UnityEvent<bool> onSideControlPanelToggled;

        [SerializeField]
        private UnityEvent<PvSoUnitAbility> onAbilitySelectedThroughButton;

        private void Start()
        {
            canvasGameObject.SetActive(false);
            onStateDetermined.RegisterCallback(ChangeViewDuringStateDetermined);
        }

        private void OnDestroy()
        {
            onStateDetermined.UnregisterCallback(ChangeViewDuringStateDetermined);
        }

        private void ChangeViewDuringStateDetermined(IEventOwner owner, UnitStateEventParam stateEventParam)
        {
            if (owner is PvMnBattleGeneralUnit battleUnit)
            {
                var state = stateEventParam.battleState;
                var stateBehaviour = stateEventParam.behaviour;

                if (stateBehaviour == UnitStateBehaviour.Clear)
                {
                    _determinedUnit = null;
                    DisableFollowingCanvas();
                }
                else if (stateBehaviour == UnitStateBehaviour.Adding)
                {
                    switch (state)
                    {
                        case UnitBattleState.UsingAbility:
                            _determinedUnit = battleUnit;
                            
                            canvasGameObject.SetActive(true);
                            canvasGameObject.transform.position = owner.Position;
                            SetupCorrectRotation(controlUiCamera);
                            mainControlPanel.gameObject.SetActive(true);
                            sideControlPanel.gameObject.SetActive(false);
                            break;
                        case UnitBattleState.Moving:
                            _determinedUnit = battleUnit;
                            DisableFollowingCanvas();
                            break;
                        case UnitBattleState.AbilityTargeting:
                            ToggleSideControlPanelAndMainControlPanel(false);
                            DisableFollowingCanvas();
                            break;
                        case UnitBattleState.AbilityConfirming:
                        case UnitBattleState.Idle:
                        case UnitBattleState.MovingProgress:
                        case UnitBattleState.Finished:
                        default:
                            DisableFollowingCanvas();
                            break;
                    }
                }
                else if (stateBehaviour == UnitStateBehaviour.Popping)
                {
                    // while Popping, the state in parameter is the state TO BE REMOVED (already removed)
                    switch (state)
                    {
                        case UnitBattleState.UsingAbility:
                            mainControlPanel.gameObject.SetActive(false);
                            // TODO: Consider side
                            DisableFollowingCanvas();
                            break;
                        case UnitBattleState.Moving:
                            _determinedUnit = null;
                            DisableFollowingCanvas();
                            throw new NotImplementedException("ERROR: Cancel from Moving still in progress");
                        case UnitBattleState.AbilityTargeting:
                            ToggleSideControlPanelAndMainControlPanel(true);
                            canvasGameObject.SetActive(true);
                            break;
                        case UnitBattleState.AbilityConfirming:
                        case UnitBattleState.Idle:
                        case UnitBattleState.MovingProgress:
                        case UnitBattleState.Finished:
                        default:
                            DisableFollowingCanvas();
                            break;
                    }
                }
            }
            else
            {
                DisableFollowingCanvas();
            }
        }

        private void DisableFollowingCanvas()
        {
            canvasGameObject.SetActive(false);
        }

        public void SetupCorrectRotation(Camera targetCamera)
        {
            controlUiCamera = targetCamera;
            canvasGameObject.transform.rotation = targetCamera.transform.rotation;
        }
        
        public void OnDeterminedAbilityColSlotSelected(bool isHostile)
        {
            if (!_determinedUnit)
            {
                throw new NullReferenceException("ERROR: No Unit Selected");
            }

            ToggleSideControlPanelAndMainControlPanel(true);
            var abilities = isHostile ? _determinedUnit.GetAttackAbilities() : _determinedUnit.GetSupportAbilities();
            sideControlPanel.NumOfSlots = abilities.Count;
            sideControlPanel.ControlButtons.ForEach(customButton =>
            {
                var refAbility = abilities[customButton.ButtonIndex];
                customButton.ButtonContentText = abilities[customButton.ButtonIndex].GetAbilityName();
                customButton.OnButtonClickedAsIndex = index =>
                {
                    onAbilitySelectedThroughButton?.Invoke(refAbility);
                };
            });
        }

        public void ToggleSideControlPanelAndMainControlPanel(bool isSideControlOn)
        {
            sideControlPanel.gameObject.SetActive(isSideControlOn);
            mainControlPanel.gameObject.SetActive(!isSideControlOn);
            
            // Invoke Side Control Panel Controlling Actions
            onSideControlPanelToggled.Invoke(isSideControlOn);
        }
    }
}
