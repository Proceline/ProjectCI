using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnFollowPanelViewer : MonoBehaviour
    {
        [SerializeField] 
        private PvSoUnitBattleStateEvent onStateDetermined;

        [SerializeField] 
        private Camera controlUiCamera;
        
        [SerializeField] 
        private GameObject canvasGameObject;

        [SerializeField]
        private PvMnControlPanel mainControlPanel;

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

                if (stateBehaviour == UnitStateBehaviour.Adding)
                {
                    switch (state)
                    {
                        case UnitBattleState.UsingAbility:
                            canvasGameObject.SetActive(true);
                            canvasGameObject.transform.position = owner.Position;
                            SetupCorrectRotation(controlUiCamera);
                            mainControlPanel.gameObject.SetActive(true);
                            break;
                        case UnitBattleState.AbilityTargeting:
                        case UnitBattleState.AbilityConfirming:
                        case UnitBattleState.Idle:
                        case UnitBattleState.Moving:
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
    }
}
