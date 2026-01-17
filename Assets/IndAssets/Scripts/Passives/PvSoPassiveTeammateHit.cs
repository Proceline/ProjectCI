using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.Services;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace IndAssets.Scripts.Passives
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "ENTJ_Commander", menuName = "ProjectCI Passives/MBTI/ENTJ_Commander", order = 1)]
    public class PvSoPassiveTeammateHit : PvSoPassiveIndividual
    {
        [Inject] private static IUnitCombatLogicPreEvent _logicPreStartedEvent;
        //[Inject] private static IUnitCombatLogicFinishedEvent _logicFinishedEvent;
        [SerializeField] private PvSoUnitAbility followUpNormalAttack;

        private static readonly ServiceLocator<FormulaCollection> FormulaService = new();
        private static FormulaCollection FormulaColInstance => FormulaService.Service;
        
        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            Debug.Log($"Initialize Passive <{name}> to {unit.name}");
            if (OwnerCount == 1)
            {
                _logicPreStartedEvent.RegisterCallback(AskTeammateToFollow);
            }
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            if (OwnerCount == 1)
            {
                _logicPreStartedEvent.UnregisterCallback(AskTeammateToFollow);
            }
        }

        private void AskTeammateToFollow(IEventOwner eventOwner, UnitAndAbilityEventParam usingParam)
        {
            if (!IsOwner(eventOwner.EventIdentifier) || eventOwner.EventIdentifier != usingParam.unit.EventIdentifier)
            {
                return;
            }

            var ownerUnit = usingParam.unit;
            var target = usingParam.target;
            var attribute = FormulaColInstance.AttackSpeedType;
            var ownerSpeed = ownerUnit.RuntimeAttributes.GetAttributeValue(attribute);
            var targetSpeed = target.RuntimeAttributes.GetAttributeValue(attribute);

            if (ownerSpeed < targetSpeed + FormulaColInstance.AttackSpeedDifference) return;

            var targetCell = target.GetCell();
            GridPawnUnit determinedUnit = null;

            foreach (var cell in targetCell.GetAllAdjacentCells())
            {
                var cellUnit = cell.GetUnitOnCell();
                if (!cellUnit) continue;
                if (TacticBattleManager.GetTeamAffinity(ownerUnit.GetTeam(), cellUnit.GetTeam()) ==
                    BattleTeam.Friendly && ownerUnit != cellUnit)
                {
                    var unitType = cellUnit.RuntimeAttributes.GetAttributeValue(FormulaColInstance.UnitTypeAttribute);
                    if (unitType == (int)UnitTypeValue.Ranged)
                    {
                        continue;
                    }

                    if (!determinedUnit)
                    {
                        determinedUnit = cellUnit;
                    }
                    else
                    {
                        var determinedSpeed = determinedUnit.RuntimeAttributes.GetAttributeValue(attribute);
                        var currentSpeed = cellUnit.RuntimeAttributes.GetAttributeValue(attribute);
                        if (currentSpeed > determinedSpeed)
                        {
                            determinedUnit = cellUnit;
                        }
                    }
                }
            }

            if (determinedUnit)
            {
                followUpNormalAttack.HandleAbilityParam(determinedUnit, target, usingParam.ResultsReference);
            }
        }
    }
}