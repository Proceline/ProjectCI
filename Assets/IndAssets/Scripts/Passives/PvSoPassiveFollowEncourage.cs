using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace IndAssets.Scripts.Passives
{
    using QueryItem = PvAbilityQueryItem<PvMnBattleGeneralUnit>;

    /// <summary>
    /// This is the passive for ENFJ Protagonist.
    /// This passive will apply support ability on a friendly adjacent unit when the owner is doing a follow-up attack.
    /// </summary>
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "Protagonist Passive", menuName = "ProjectCI Passives/MBTI/Protagonist_ENFJ", order = 1)]
    public class PvSoPassiveFollowEncourage : PvSoPassiveIndividual
    {
        [Inject] internal static readonly IUnitGeneralCombatingEvent OnCombatingListCreatedEvent;
        [Inject] internal static readonly IUnitCombatingQueryEndEvent OnCombatingListFinishedEvent;

        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            OnCombatingListCreatedEvent.RegisterCallback(AdjustFollowUpCondition);
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            OnCombatingListCreatedEvent.UnregisterCallback(AdjustFollowUpCondition);
        }
        
        private void AdjustFollowUpCondition(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget, List<QueryItem> queryItems)
        {
            if (!IsOwner(inUnit.ID))
            {
                return;
            }

            var followUpIndex = queryItems.FindIndex
                (query => query.queryOrderForm.HasFlag(PvEnDamageForm.FollowUp) 
                && !query.queryOrderForm.HasFlag(PvEnDamageForm.Counter));

            if (followUpIndex < 0 || !queryItems[followUpIndex].enabled || inUnit.IsDead())
            {
                return;
            }

            var queryItem = QueryItem.CreateQueryItemIntoList(queryItems, 0);
            queryItem.SetAbility(inUnit.SupportAbility, PvEnDamageForm.Support);
            queryItem.holdingOwner = inUnit;
            if (!ApplySupportOnNeighbors(inUnit, queryItem))
            {
                queryItem.enabled = false;
            }
        }

        private bool ApplySupportOnNeighbors(PvMnBattleGeneralUnit fromUnit, QueryItem queryItem)
        {
            queryItem.SetAbility(fromUnit.SupportAbility, PvEnDamageForm.Support);

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
                return false;
            }

            queryItem.targetUnit = firstFriendCell.GetUnitOnCell() as PvMnBattleGeneralUnit;
            queryItem.enabled = true;

            return true;
        }
    }
}