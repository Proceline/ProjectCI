using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI
{
    public class EndTurnUIElementBase : MonoBehaviour
    {
        public Button m_Button;

        private void Start()
        {
            TacticBattleManager.Get().OnTeamWon.AddListener(HandleGameDone);
        }

        void Update()
        {
            m_Button.interactable = TacticBattleManager.CanFinishTurn();
        }
        
        public void EndTurn()
        {
            TacticBattleManager.FinishTurn();
        }

        void HandleGameDone(BattleTeam InWinningTeam)
        {
            gameObject.SetActive(false);
        }
    }
}
