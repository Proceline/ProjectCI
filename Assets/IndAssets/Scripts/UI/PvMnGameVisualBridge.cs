using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using System;
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

        [SerializeField]
        private PvSoLevelCellEvent onHoverCellEventWithOwner;

        [SerializeField]
        private PvSoBattleState onBattleState;

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
            onHoverCellEventWithOwner.RegisterCallback(CreatePreviewForTarget);
            onBattleState.RegisterCallbackOnEnter(TogglePreviewOnState);
        }

        private void OnDestroy()
        {
            battleState.UnregisterCallbackOnEnter(OnBattleStateEntered);
            roundSwitchEvent.UnregisterCallback(OnRoundSwitchResponse);
            onHoverCellEventWithoutOwner.UnregisterCallback(CreatePreviewForUnit);
            onHoverCellEventWithOwner.UnregisterCallback(CreatePreviewForTarget);
            onBattleState.UnregisterCallbackOnEnter(TogglePreviewOnState);
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
            CreatePreview(unit, 0);
        }

        private void CreatePreviewForTarget(LevelCellBase target)
        {
            if (target)
            {
                var unit = target.GetUnitOnCell();
                CreatePreview(unit, 1);
            }
            else if (_pawnPreviews[1] && _pawnPreviews[1].gameObject.activeSelf)
            {
                _pawnsInView[1] = null;
                _pawnPreviews[1].gameObject.SetActive(false);
            }
        }

        private void TogglePreviewOnState(PvPlayerRoundState state, PvMnBattleGeneralUnit unit)
        {
            switch (state)
            {
                case PvPlayerRoundState.None:
                    CreatePreviewForTarget(null);
                    break;
                default:
                    break;
            }
        }

        private void CreatePreview(GridPawnUnit unit, int index)
        {
            if (unit)
            {
                if (!_pawnPreviews[index])
                {
                    _pawnPreviews[index] = Instantiate(pawnPreviewPanelPrefab, pawnPreviewParents[index]);
                }

                if (!_pawnPreviews[index].gameObject.activeSelf)
                {
                    _pawnPreviews[index].gameObject.SetActive(true);
                }

                if (!_pawnsInView[index] || _pawnsInView[index] != unit)
                {
                    _pawnPreviews[index].Setup(unit);
                    _pawnsInView[index] = unit;
                }
            }
            else
            {
                if (_pawnPreviews[index] && _pawnPreviews[index].gameObject.activeSelf)
                {
                    _pawnPreviews[index].gameObject.SetActive(false);
                }
            }
        }
    }
}