using System;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
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
            _logicFinishedEvent.RegisterCallback(unit.AddExtraFollowUp);
        }

        protected override void DisposePassiveInternally(GridPawnUnit unit)
        {
            _logicFinishedEvent.UnregisterCallback(unit.AddExtraFollowUp);
        }
    }
    
    internal static class ExtraAttackExtForUnitSelf
    {
        internal static void AddExtraFollowUp(this GridPawnUnit ownerUnit, IEventOwner eventOwner,
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
            
            var results = usingParam.ResultsReference;
            var caster = usingParam.unit;
            var target = usingParam.target;
            
            ability.HandleAbilityParam(caster, target, results);
        }
    }
}