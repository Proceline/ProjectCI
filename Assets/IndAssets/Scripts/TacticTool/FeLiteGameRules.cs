using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using System;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine.Events;
using IndAssets.Scripts.Managers;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "NewGameRules", menuName = "ProjectCI Tools/GameRules/Create FeLiteGameRules", order = 1)]
    public partial class FeLiteGameRules : BattleGameRules
    {
        [SerializeField]
        private PvSoUnitAbility[] allAbilities;
        private readonly Dictionary<string, PvMnBattleGeneralUnit> _unitIdToBattleUnitHash = new();
        private readonly Dictionary<string, PvSoUnitAbility> _abilityIdToAbilityHash = new();

        [NonSerialized] private PvMnBattleGeneralUnit _selectedUnit;

        [SerializeField]
        private LayerMask[] layerMasksRuleList;

        [Header("Update Support")]
        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onUpdateSupport;

        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onUpdateSupportWithAbility;

        [Header("Select Support Event View-Only")]

        /// <summary>
        /// View-only Event Raiser while Turn Owner Selected
        /// </summary>
        [SerializeField]
        private PvSoUnitSelectEvent raiserOwnerSelectedViewEvent;

        /// <summary>
        /// View-only Response while Turn Owner Selected
        /// </summary>
        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onTurnOwnerSelectedPreview;

        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onTurnOwnerDeSelectedPreview;

        [Header("On Turn Support"), SerializeField]
        private PvSoTurnViewEndEvent raiserTurnLockerEvent;

        #region Injected Fields

        public static PvSoSimpleDamageApplyEvent XRaiserSimpleDamageApplyEvent
        {
            get
            {
                try
                {
                    return RaiserSimpleDamageApplyEvent;
                }
                catch
                {
                    throw new NullReferenceException("ERROR: FeLiteGameRules didn't involved injection!");
                }
            }
        }

        [Inject] private static readonly PvSoSimpleDamageApplyEvent RaiserSimpleDamageApplyEvent;
        [Inject] private static readonly IUnitPrepareEvent RaiserManualFinishOrRestPrepareEvent;

        [Inject] private static readonly ITeamRoundEndEvent RaiserTeamRoundEndEvent;
        [SerializeField] private List<UnityEvent<BattleTeam, List<float>>> roundEventEndList;
        private readonly List<float> _teamRoundEndDelayList = new();

        [Inject] private static readonly IUnitGeneralCombatingEvent RaiserOnCombatingListCreatedEvent;
        [Inject] private static readonly IUnitCombatingQueryEndEvent RaiserOnCombatingQueryEndEvent;

        [Inject] private static readonly IAnimationOutLengthFunc GetPresetAnimationLengthFunc;
        [Inject] private static readonly IAnimationOutBreakPointFunc GetPresetAnimationBreakPointFunc;
        [Inject] private static readonly PvSoAnimationTriggerEvent RaiserAnimationPlayEvent;

        #endregion

        protected override void StartGame()
        {
            gameBattleState.Clear();

            _finishedPlayableUnits.Clear();
            _unitIdToBattleUnitHash.Clear();
            _abilityIdToAbilityHash.Clear();

            CurrentTeam = BattleTeam.Friendly;

            foreach (var singleAbility in allAbilities)
            {
                if (string.IsNullOrEmpty(singleAbility.ID))
                {
                    singleAbility.GenerateNewID();
                }
                _abilityIdToAbilityHash.TryAdd(singleAbility.ID, singleAbility);
            }

            var units = FindObjectsByType<PvMnBattleGeneralUnit>(FindObjectsSortMode.None);
            foreach (var unit in units)
            {
                _unitIdToBattleUnitHash.TryAdd(unit.ID, unit);
            }

            TacticBattleManager.HandleGameStarted();

            BeginTeamTurn(CurrentTeam);
        }

        private void SetupTeam(BattleTeam inTeam)
        {
            List<GridPawnUnit> units = TacticBattleManager.GetUnitsOnTeam(inTeam);
            foreach (GridPawnUnit unit in units)
            {
                unit.HandleTurnStarted();
            }
        }

        public override void Update()
        {
            switch (gameBattleState.GetCurrentState)
            {
                case PvPlayerRoundState.Selected:
                case PvPlayerRoundState.Moving:
                case PvPlayerRoundState.None:
                    onUpdateSupport.Invoke(_selectedUnit);
                    break;
                case PvPlayerRoundState.Prepare:
                    onUpdateSupportWithAbility.Invoke(_selectedUnit);
                    break;
            }
        }

        public override GridPawnUnit GetSelectedUnit()
        {
            return _selectedUnit;
        }

        public override void BeginTeamTurn(BattleTeam inTeam)
        {
            gameBattleState.Clear();

            SetupTeam(inTeam);

            if (inTeam == BattleTeam.Hostile)
            {
                bool bIsHostileTeamAI = TacticBattleManager.IsTeamAI(BattleTeam.Hostile);
                if (bIsHostileTeamAI)
                {
                    List<GridPawnUnit> aiUnits = TacticBattleManager.GetUnitsOnTeam(BattleTeam.Hostile);
                    // AStarAlgorithmUtils.RunAI(aiUnits, EndTurn);
                    // TODO: Run AI
                }
            }
        }

        /// <summary>
        /// Create Cell according to Rules, used in Unity Event, don't delete this function
        /// </summary>
        /// <param name="hit">rayCast information</param>
        /// <param name="keyIndex">Grid Cell Index</param>
        /// <param name="grid">new created grid</param>
        public void ApplyRuleOnCellCreating(RaycastHit hit, Vector2Int keyIndex, LevelGridBase grid)
        {
            int layerFlagValue = Mathf.RoundToInt(Mathf.Pow(2, hit.collider.gameObject.layer));
            if (layerFlagValue == layerMasksRuleList[0])
            {
                var cell = grid.GenerateCell(hit.point, keyIndex);
                cell.Reset();
            }
        }

        /// <summary>
        /// Handles the logic required when a unit has been defeated in battle.
        /// Used in asset _OnUnitDeathOfficiallyTriggeredEvent.asset, as binding UnityAction
        /// </summary>
        /// <param name="deadUnit">The unit that has been defeated. Cannot be null.</param>
        public void HandleDeadUnit(PvMnBattleGeneralUnit deadUnit)
        {
            if (!deadUnit)
            {
                return;
            }

            deadUnit.SetCurrentCell(null);

            _unitIdToBattleUnitHash.Remove(deadUnit.ID);
            deadUnit.DoActionOnInstalledPassives(passive => passive.DisposePassive(deadUnit));
            deadUnit.CleanUpPassives();
        }
    }
}