using ProjectCI.CoreSystem.DependencyInjection;
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
    [CreateAssetMenu(fileName = "New Extra Attack Passive", menuName = "ProjectCI Passives/ExtraAttack", order = 1)]
    public class PvSoPassiveTeammateHit : PvSoPassiveBase
    {
        [Inject] private static IUnitCombatLogicFinishedEvent _logicFinishedEvent;
        
        private static readonly ServiceLocator<FormulaCollection> FormulaService = new();
        internal static FormulaCollection FormulaColInstance => FormulaService.Service;
        
        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            Debug.Log($"Initialize Passive <{name}> to {unit.name}");
            _logicFinishedEvent.RegisterCallback(unit.AskTeammateToFollow);
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            _logicFinishedEvent.UnregisterCallback(unit.AskTeammateToFollow);
        }
    }
    
    internal static class ExtraAttackExtForUnitSelf
    {
        
        internal static void AskTeammateToFollow(this GridPawnUnit ownerUnit, IEventOwner eventOwner,
            UnitAndAbilityEventParam usingParam)
        {
            if (ownerUnit != usingParam.unit)
            {
                return;
            }
            
            var ability = usingParam.ability;
            if (!ability.IsFollowUpAllowed())
            {
                return;
            }

            var attribute = PvSoPassiveTeammateHit.FormulaColInstance.AttackSpeedType;
            var ownerSpeed = ownerUnit.RuntimeAttributes.GetAttributeValue(attribute);
            var target = usingParam.target;
            var targetSpeed = target.RuntimeAttributes.GetAttributeValue(attribute);

            if (ownerSpeed < targetSpeed + 5) return;
            
            var results = usingParam.ResultsReference;
            
            var targetCell = target.GetCell();
            foreach (var cell in targetCell.GetAllAdjacentCells())
            {
                var cellUnit = cell.GetUnitOnCell();
                if (!cellUnit) continue;
                if (TacticBattleManager.GetTeamAffinity(ownerUnit.GetTeam(), cellUnit.GetTeam()) ==
                    BattleTeam.Friendly && ownerUnit != cellUnit)
                {
                    ability.HandleAbilityParam(cellUnit, target, results);
                }
            }
        }
    }
}