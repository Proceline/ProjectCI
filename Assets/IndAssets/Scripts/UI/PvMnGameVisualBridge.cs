using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectCI.CoreSystem.Runtime.UI
{
    public class PvMnGameVisualBridge : MonoBehaviour
    {
        [Header("In Scene Assets"), SerializeField]
        private Button roundEndButton;

        [SerializeField]
        private PvUIHoverPawnInfo pawnPreviewPanelPrefab;

        [SerializeField]
        private Transform[] pawnPreviewParents = new Transform[2];

        [Header("Global Assets"), SerializeField]
        private PvSoBattleState battleState;

        [SerializeField]
        private PvSoBattleTeamEvent roundSwitchEvent;

        [SerializeField]
        private PvSoLevelCellEvent onHoverCellEventWithoutOwner;

        private readonly PvUIHoverPawnInfo[] _pawnPreviews = new PvUIHoverPawnInfo[2];
        private readonly GridPawnUnit[] _pawnsInView = new GridPawnUnit[2];

        private void Awake()
        {
            roundEndButton.gameObject.SetActive(false);
        }

        private void Start()
        {
            battleState.RegisterCallbackOnEnter(OnBattleStateEntered);
            roundSwitchEvent.RegisterCallback(OnRoundSwitchResponse);
            onHoverCellEventWithoutOwner.RegisterCallback(CreatePreviewForUnit);
        }

        private void OnDestroy()
        {
            battleState.UnregisterCallbackOnEnter(OnBattleStateEntered);
            roundSwitchEvent.UnregisterCallback(OnRoundSwitchResponse);
            onHoverCellEventWithoutOwner.UnregisterCallback(CreatePreviewForUnit);
        }

        private void OnBattleStateEntered(PvPlayerRoundState state, PvMnBattleGeneralUnit unit)
        {
            roundEndButton.gameObject.SetActive(state == PvPlayerRoundState.None);
        }

        private void OnRoundSwitchResponse(BattleTeam endingTeam)
        {
            roundEndButton.gameObject.gameObject.SetActive(endingTeam != BattleTeam.Friendly);
        }

        private void CreatePreviewForUnit(LevelCellBase cell)
        {
            var unit = cell.GetUnitOnCell();

            if (unit)
            {
                if (!_pawnPreviews[0])
                {
                    _pawnPreviews[0] = Instantiate(pawnPreviewPanelPrefab, pawnPreviewParents[0]);
                }
                
                if (!_pawnPreviews[0].gameObject.activeSelf)
                {
                    _pawnPreviews[0].gameObject.SetActive(true);
                }

                if (!_pawnsInView[0] || _pawnsInView[0] != unit)
                {
                    _pawnPreviews[0].Setup(unit);
                    _pawnsInView[0] = unit;
                }
            }
            else
            {
                if (_pawnPreviews[0] && _pawnPreviews[0].gameObject.activeSelf)
                {
                    _pawnPreviews[0].gameObject.SetActive(false);
                }
            }
        }
    }
}