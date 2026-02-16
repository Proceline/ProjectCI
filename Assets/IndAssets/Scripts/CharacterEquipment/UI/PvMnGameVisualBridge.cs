using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectCI.CoreSystem.Runtime.UI
{
    public class PvMnGameVisualBridge : MonoBehaviour
    {
        [SerializeField]
        private Button roundEndButton;

        [SerializeField]
        private PvSoBattleState battleState;

        [SerializeField]
        private PvSoBattleTeamEvent roundSwitchEvent;

        private void Awake()
        {
            roundEndButton.gameObject.SetActive(false);
        }

        private void Start()
        {
            battleState.RegisterCallbackOnEnter(OnBattleStateEntered);
            roundSwitchEvent.RegisterCallback(OnRoundSwitchResponse);
        }

        private void OnDestroy()
        {
            battleState.UnregisterCallbackOnEnter(OnBattleStateEntered);
            roundSwitchEvent.UnregisterCallback(OnRoundSwitchResponse);
        }

        private void OnBattleStateEntered(PvPlayerRoundState state, PvMnBattleGeneralUnit unit)
        {
            roundEndButton.gameObject.SetActive(state == PvPlayerRoundState.None);
        }

        private void OnRoundSwitchResponse(BattleTeam endingTeam)
        {
            roundEndButton.gameObject.gameObject.SetActive(endingTeam != BattleTeam.Friendly);
        }
    }
}