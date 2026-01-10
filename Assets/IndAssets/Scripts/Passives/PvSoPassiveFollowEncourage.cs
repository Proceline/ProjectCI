using System;
using System.Collections.Generic;
using IndAssets.Scripts.Events;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace IndAssets.Scripts.Passives
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "Protagonist Passive", menuName = "ProjectCI Passives/MBTI/Protagonist_ENFJ", order = 1)]
    public class PvSoPassiveFollowEncourage : PvSoPassiveIndividual
    {
        [Inject] private static readonly IUnitGeneralCombatingEvent OnCombatingListCreatedEvent;
        
        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            var info = new CombatingQueryContext
            {
                IsCounter = false,
                QueryType = CombatingQueryType.ReplacedFollowUp
            };
            
            OnCombatingListCreatedEvent.RegisterCallback(AdjustFollowUpCondition);
            unit.RegisterQueryApply(info, ApplySupportOnNeighbors);
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            var info = new CombatingQueryContext
            {
                IsCounter = false,
                QueryType = CombatingQueryType.ReplacedFollowUp
            };
            
            OnCombatingListCreatedEvent.UnregisterCallback(AdjustFollowUpCondition);
            unit.UnregisterQueryApply(info);
        }
        
        private void AdjustFollowUpCondition(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<CombatingQueryContext> queryContexts)
        {
            if (!IsOwner(inUnit.ID))
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

            var ability = fromUnit.GetSupportAbilities()[0];

            var resultId = Guid.NewGuid().ToString();
            var effectedCells = fromUnit.GetCell().GetAllAdjacentCells();
            var firstFriendCell = effectedCells.Find(cell =>
            {
                var cellUnit = cell.GetUnitOnCell();
                if (!cellUnit) return false;
                return TacticBattleManager.GetTeamAffinity(fromUnit.GetTeam(), cellUnit.GetTeam()) ==
                    BattleTeam.Friendly && fromUnit != cellUnit;
            });

            if (!firstFriendCell)
            {
                return;
            }

            foreach (var param in ability.GetParameters())
            {
                var cellUnit = firstFriendCell.GetUnitOnCell();
                if (cellUnit)
                {
                    if (TacticBattleManager.GetTeamAffinity(fromUnit.GetTeam(), cellUnit.GetTeam()) ==
                        BattleTeam.Friendly && fromUnit != cellUnit)
                    {
                        param.Execute(resultId, ability, fromUnit, cellUnit, firstFriendCell, commands, 0);
                    }
                }
            }
        }
    }
}