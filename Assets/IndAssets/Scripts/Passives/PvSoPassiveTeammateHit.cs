using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.TacticTool.Formula.Concrete;
using System.Collections.Generic;
using UnityEngine;

namespace IndAssets.Scripts.Passives
{
    using QueryItem = PvAbilityQueryItem<PvMnBattleGeneralUnit>;

    [CreateAssetMenu(fileName = "ENTJ_Commander", menuName = "ProjectCI Passives/MBTI/ENTJ_Commander", order = 1)]
    public class PvSoPassiveTeammateHit : PvSoPassiveIndividual
    {
        private static readonly ServiceLocator<FormulaCollection> FormulaService = new();
        private static FormulaCollection FormulaColInstance => FormulaService.Service;

        protected override void InstallPassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            PvSoPassiveFollowEncourage.OnCombatingListCreatedEvent.RegisterCallback(AdjustFollowUpCondition);
        }

        protected override void DisposePassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            PvSoPassiveFollowEncourage.OnCombatingListCreatedEvent.UnregisterCallback(AdjustFollowUpCondition);
        }

        protected override void InstallPassivePersonally(PvMnBattleGeneralUnit unit)
        {
            // Empty
        }

        protected override void DisposePassivePersonally(PvMnBattleGeneralUnit unit)
        {
            // Empty
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

            var targetCell = inTarget.GetCell();
            GridPawnUnit determinedUnit = null;
            var speedAttribute = FormulaColInstance.AttackSpeedType;

            foreach (var cell in targetCell.GetAllAdjacentCells())
            {
                var cellUnit = cell.GetUnitOnCell();
                if (!cellUnit) continue;
                if (TacticBattleManager.GetTeamAffinity(inUnit.GetTeam(), cellUnit.GetTeam()) ==
                    BattleTeam.Friendly && inUnit != cellUnit)
                {
                    if (!determinedUnit)
                    {
                        determinedUnit = cellUnit;
                    }
                    else
                    {
                        var determinedSpeed = determinedUnit.RuntimeAttributes.GetAttributeValue(speedAttribute);
                        var currentSpeed = cellUnit.RuntimeAttributes.GetAttributeValue(speedAttribute);
                        if (currentSpeed > determinedSpeed)
                        {
                            determinedUnit = cellUnit;
                        }
                    }
                }
            }

            if (determinedUnit)
            {
                var queryItem = QueryItem.CreateQueryItemIntoList(queryItems, 0);
                var castedUnit = determinedUnit as PvMnBattleGeneralUnit;
                queryItem.SetAbility(castedUnit.FollowUpAbility, PvEnDamageForm.Aggressive);
                queryItem.holdingOwner = castedUnit;
                queryItem.targetUnit = inTarget;
            }
        }
    }
}