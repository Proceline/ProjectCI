using IndAssets.Scripts.Managers;
using IndAssets.Scripts.TacticTool;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

        [SerializeField]
        private GameObject abilitiesPanel;

        [SerializeField]
        private Image[] abilityHints = new Image[3];

        [SerializeField]
        private Image[] existedUltSymbols = new Image[2];

        [SerializeField]
        private GameObject existedUltSymbolsContainer;

        [SerializeField]
        private Transform ultSymbolsParent;

        private readonly List<(Image, Image)> _ultSymbols = new();

        [NonSerialized]
        private bool _ultPanelInitialized = false;

        [SerializeField]
        private GameObject onStageClearRoot;

        [Header("Global Assets"), SerializeField]
        private PvSoBattleTeamEvent roundSwitchEvent;

        [SerializeField]
        private PvSoBattleTeamEvent roundStartEvent;

        [SerializeField]
        private PvSoLevelCellEvent onHoverCellEventWithoutOwner;

        [SerializeField]
        private PvSoLevelCellEvent onHoverCellEventWithOwner;

        [SerializeField]
        private PvSoBattleState onBattleState;

        [SerializeField]
        private PvSoSimpleVoidEvent onGamePreEndedEvent;

        [SerializeField]
        private PvSoUnitsDictionary unitIdsToBattleUnitHash;
        private readonly List<PvMnBattleGeneralUnit> _bufferedUltableUnits = new();

        private ITeamRoundEndEvent OnRoundEndedEvent => roundSwitchEvent;
        private ITeamRoundStartEvent OnRoundStartedEvent => roundStartEvent;

        [Header("Evnets"), SerializeField]
        private UnityEvent<Dictionary<GridPawnUnit, int>, LevelCellBase> onCombatOutPreviewsEvent;

        private readonly PvUIHoverPawnInfo[] _pawnPreviews = new PvUIHoverPawnInfo[2];
        private readonly GridPawnUnit[] _pawnsInView = new GridPawnUnit[2];

        private readonly Dictionary<GridPawnUnit, int> _combatingPreviewResults = new();

        private void Awake()
        {
            roundEndButton.gameObject.SetActive(false);
            abilitiesPanel.SetActive(false);
            _ultSymbols.Add((existedUltSymbols[0], existedUltSymbols[1]));
        }

        private void Start()
        {
            onBattleState.RegisterCallbackOnEnter(OnBattleStateEntered);
            OnRoundEndedEvent.RegisterCallback(OnRoundSwitchResponse);
            OnRoundStartedEvent.RegisterCallback(OnRoundStartedResponse);

            onHoverCellEventWithoutOwner.RegisterCallback(CreatePreviewForUnit);
            onHoverCellEventWithOwner.RegisterCallback(CreatePreviewForTarget);
            onBattleState.RegisterCallbackOnEnter(TogglePreviewOnState);
            onGamePreEndedEvent.RegisterCallback(EnableStageClear);
        }

        private void OnDestroy()
        {
            onBattleState.UnregisterCallbackOnEnter(OnBattleStateEntered);
            OnRoundEndedEvent.UnregisterCallback(OnRoundSwitchResponse);
            OnRoundStartedEvent.UnregisterCallback(OnRoundStartedResponse);

            onHoverCellEventWithoutOwner.UnregisterCallback(CreatePreviewForUnit);
            onHoverCellEventWithOwner.UnregisterCallback(CreatePreviewForTarget);
            onBattleState.UnregisterCallbackOnEnter(TogglePreviewOnState);
            onGamePreEndedEvent.UnregisterCallback(EnableStageClear);
        }

        private void OnBattleStateEntered(PvPlayerRoundState state, PvMnBattleGeneralUnit unit)
        {
            var atNoneState = state == PvPlayerRoundState.None;
            roundEndButton.gameObject.SetActive(atNoneState); 
            ultSymbolsParent.gameObject.SetActive(atNoneState);
        }

        private void OnRoundSwitchResponse(BattleTeam endingTeam)
        {
            roundEndButton.gameObject.gameObject.SetActive(endingTeam != BattleTeam.Friendly);
        }

        private void OnRoundStartedResponse(BattleTeam endingTeam)
        {
            ultSymbolsParent.gameObject.SetActive(endingTeam == BattleTeam.Friendly);

            if (!_ultPanelInitialized)
            {
                _bufferedUltableUnits.Clear();

                foreach (var itemPair in unitIdsToBattleUnitHash)
                {
                    var value = itemPair.Value;

                    if (value.GetTeam() == BattleTeam.Friendly)
                    {
                        _bufferedUltableUnits.Add(value);
                    }
                }
                _ultPanelInitialized = true;
            }

            if (_bufferedUltableUnits.Count != _ultSymbols.Count)
            {
                for (var i = _ultSymbols.Count; i < _bufferedUltableUnits.Count; i++)
                {
                    var container = Instantiate(existedUltSymbolsContainer, ultSymbolsParent);
                    var images = container.GetComponentsInChildren<Image>();
                    _ultSymbols.Add((images[1], images[2]));
                }
            }

            for (var i = 0; i < _bufferedUltableUnits.Count; i++)
            {
                _ultSymbols[i].Item1.transform.parent.gameObject.SetActive(!_bufferedUltableUnits[i].IsDead());

                var sprite = _bufferedUltableUnits[i].UltimateAbility.GetIconSprite;
                _ultSymbols[i].Item1.sprite = sprite;
                _ultSymbols[i].Item2.sprite = sprite;
            }
        }

        private void CreatePreviewForUnit(LevelCellBase cell)
        {
            var unit = cell.GetUnitOnCell();
            if (CreatePreview(unit, 0) && unit)
            {
                SetupUnitAbilities(unit);
                abilitiesPanel.SetActive(true);
            }
            else if (!unit)
            {
                abilitiesPanel.SetActive(false);
            }
        }

        private void CreatePreviewForTarget(LevelCellBase target)
        {
            if (target)
            {
                var unit = target.GetUnitOnCell();
                if (CreatePreview(unit, 1))
                {
                    onCombatOutPreviewsEvent.Invoke(_combatingPreviewResults, target);

                    if (_combatingPreviewResults.TryGetValue(_pawnsInView[0], out var deltaOnOwner))
                    {
                        _pawnPreviews[0].Setup(_pawnsInView[0], deltaOnOwner);
                    }
                    else
                    {
                        _pawnPreviews[0].Setup(_pawnsInView[0], 0);
                    }

                    if (_combatingPreviewResults.TryGetValue(_pawnsInView[1], out var deltaOnTarget))
                    {
                        _pawnPreviews[1].Setup(_pawnsInView[1], deltaOnTarget);
                    }
                    else
                    {
                        _pawnPreviews[1].Setup(_pawnsInView[1], 0);
                    }
                }
            }
            else if (_pawnPreviews[1] && _pawnPreviews[1].gameObject.activeSelf)
            {
                _pawnPreviews[0].Setup(_pawnsInView[0], 0);
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

        /// <summary>
        /// Create and Check if Preview refreshed
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="index"></param>
        /// <returns>If Preview updated on Canvas</returns>
        private bool CreatePreview(GridPawnUnit unit, int index)
        {
            if (unit)
            {
                if (!_pawnPreviews[index])
                {
                    _pawnPreviews[index] = Instantiate(pawnPreviewPanelPrefab, pawnPreviewParents[index]);
                }

                var activateThisFrame = false;

                if (!_pawnPreviews[index].gameObject.activeSelf)
                {
                    _pawnPreviews[index].gameObject.SetActive(true);
                    activateThisFrame = true;
                }

                if (!_pawnsInView[index] || _pawnsInView[index] != unit)
                {
                    _pawnPreviews[index].Setup(unit, 0);
                    _pawnsInView[index] = unit;
                    activateThisFrame = true;
                }

                return activateThisFrame;
            }
            else
            {
                if (_pawnPreviews[index] && _pawnPreviews[index].gameObject.activeSelf)
                {
                    _pawnPreviews[index].gameObject.SetActive(false);
                }

                return false;
            }
        }

        private void SetupUnitAbilities(GridPawnUnit unit)
        {
            if (unit is PvMnBattleGeneralUnit battleUnit)
            {
                abilityHints[0].sprite = battleUnit.AttackAbility.GetIconSprite;
                abilityHints[1].sprite = battleUnit.FollowUpAbility.GetIconSprite;
                abilityHints[2].sprite = battleUnit.SupportAbility.GetIconSprite;
            }
        }

        private void EnableStageClear()
        {
            onStageClearRoot.SetActive(true);
        }
    }
}