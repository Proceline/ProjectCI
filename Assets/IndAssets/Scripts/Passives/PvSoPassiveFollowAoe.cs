using IndAssets.Scripts.Events;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using System.Collections.Generic;
using UnityEngine;

namespace IndAssets.Scripts.Passives
{
    [CreateAssetMenu(fileName = "PvSoPassiveFollowAoe", menuName = "ProjectCI Passives/PvSoPassiveFollowAoe", order = 1)]
    public class PvSoPassiveFollowAoe : PvSoPassiveIndividual
    {
        private static readonly ServiceLocator<FormulaCollection> FormulaService = new();
        internal static FormulaCollection FormulaColInstance => FormulaService.Service;

        [SerializeField]
        private PvSoUnitAbility followUpAbility;

        [SerializeField]
        private bool targetToSelf = false;

        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            var info = new CombatingQueryContext
            {
                IsCounter = false,
                QueryType = CombatingQueryType.ReplacedFollowUp
            };

            PvSoPassiveFollowEncourage.OnCombatingListCreatedEvent.RegisterCallback(AdjustFollowUpToAoe);
            unit.RegisterQueryApply(info, ApplySupportOnNeighbors);
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            var info = new CombatingQueryContext
            {
                IsCounter = false,
                QueryType = CombatingQueryType.ReplacedFollowUp
            };

            PvSoPassiveFollowEncourage.OnCombatingListCreatedEvent.UnregisterCallback(AdjustFollowUpToAoe);
            unit.UnregisterQueryApply(info);
        }

        private void AdjustFollowUpToAoe(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<CombatingQueryContext> queryContexts)
        {
            if (!IsOwner(inUnit.ID))
            {
                return;
            }

            var neighbors = inUnit.GetCell().GetAllAdjacentCells();
            var friendlyNeighborsCount = 0;
            foreach (var neighborCell in neighbors)
            {
                if (TacticBattleManager.GetTeamAffinity(inUnit.GetTeam(), neighborCell.GetCellTeam()) 
                    == BattleTeam.Friendly)
                {
                    friendlyNeighborsCount++;
                }
            }
            if (friendlyNeighborsCount < 2)
            {
                return;
            }

            var followUpIndex = queryContexts.FindIndex(query => query.QueryType == CombatingQueryType.AutoFollowUp);
            if (followUpIndex < 0)
            {
                return;
            }

            var replacedQuery = queryContexts[followUpIndex];
            replacedQuery.QueryType = CombatingQueryType.ReplacedFollowUp;
            queryContexts[followUpIndex] = replacedQuery;
        }

        private void ApplySupportOnNeighbors(PvMnBattleGeneralUnit fromUnit, PvMnBattleGeneralUnit toUnit,
            Queue<CommandResult> commands)
        {
            if (fromUnit.IsDead())
            {
                return;
            }

            foreach (var param in followUpAbility.GetParameters())
            {
                followUpAbility.HandleAbilityParam(fromUnit, targetToSelf ? fromUnit : toUnit, commands);
            }
        }
    }
}