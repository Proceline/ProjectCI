using System;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Passives
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "New Extra Attack Passive", menuName = "ProjectCI Passives/ExtraAttack", order = 1)]
    public class PvSoPassiveExtraAttack : PvSoPassiveBase
    {
        [Inject] private static IUnitCombatLogicFinishedEvent _logicFinishedEvent;
        
        protected override void InstallPassiveInternally(GridPawnUnit unit)
        {
            Debug.Log($"Initialize Passive <{name}> to {unit.name}");
            _logicFinishedEvent.RegisterCallback(AddExtraFollowUp);
        }

        protected override void DisposePassiveInternally(GridPawnUnit unit)
        {
            _logicFinishedEvent.UnregisterCallback(AddExtraFollowUp);
        }

        private void AddExtraFollowUp(IEventOwner eventOwner, UnitAndAbilityEventParam usingParam)
        {
            if (!IsOwner(eventOwner.EventIdentifier))
            {
                return;
            }

            var ability = usingParam.ability;
            if (!ability.IsFollowUpAllowed())
            {
                return;
            }
            
            var results = usingParam.ResultsReference;

            if (!ability)
            {
                throw new NullReferenceException("ERROR: One of these two pawns missing ability!");
            }

            var caster = usingParam.unit;
            var target = usingParam.target;

            // Because already allowed FollowUp, Thus no need to check reachable
            // List<LevelCellBase> reachableCells = ability.GetAbilityCells(caster);
            // var bIsTargetAbilityAbleToCounter = reachableCells.Count > 0 && reachableCells.Contains(target.GetCell());
            
            FeLiteGameRules.HandleAbilityParam(ability, caster, target, results);
        }
    }
}