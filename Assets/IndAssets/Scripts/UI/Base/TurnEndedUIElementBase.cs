using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI
{
    public class TurnEndedUIElementBase : MonoBehaviour
    {
        public GameObject m_Screen;
        public RawImage m_Image;
        public Texture2D m_FriendlyTurn;
        public Texture2D m_HostileTurn;

        bool m_bShowing = false;
        Queue<BattleTeam> TeamQueue = new Queue<BattleTeam>();

        void Start()
        {
            TacticBattleManager.Get().OnTeamWon.AddListener(HandleGameDone);

            BattleGameRules gameRules = TacticBattleManager.GetRules();
            if(gameRules)
            {
                gameRules.OnTeamTurnBegin.AddListener(HandleTeamChanged);
            }

            m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, 0.0f);
        }

        void HandleTeamChanged(BattleTeam InTeam)
        {
            if(InTeam == BattleTeam.Friendly || InTeam == BattleTeam.Hostile)
            {
                if(m_bShowing)
                {
                    TeamQueue.Enqueue(InTeam);
                }
                else
                {
                    m_Image.texture = InTeam == BattleTeam.Friendly ? m_FriendlyTurn : m_HostileTurn;
                    m_bShowing = true;
                    StartCoroutine(HandleShowGraphic());
                }
            }
        }

        IEnumerator HandleShowGraphic()
        {
            m_bShowing = true;

            m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, 1.0f);

            yield return new WaitForSeconds(0.5f);

            m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, 0.0f);

            m_bShowing = false;

            if (TeamQueue.Count > 0)
            {
                HandleTeamChanged(TeamQueue.Dequeue());
            }
        }


        void HandleGameDone(BattleTeam InWinningTeam)
        {
            m_Screen.SetActive(false);
        }
    }
}
