using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Abilities.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Passives
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "New Support Attack Passive", menuName = "ProjectCI Passives/SupportAttack", order = 1)]
    public class PvSoPassiveSupportAttack : PvSoPassiveBase
    {
        [Inject] private static IUnitCombatLogicPreEvent _logicPreStartedEvent;

        protected override void InstallPassiveInternally(GridPawnUnit unit)
        {
            Debug.Log($"Initialize Passive <{name}> to {unit.name}");
            if (unit is PvMnBattleGeneralUnit castedUnit)
            {
                _logicPreStartedEvent.RegisterCallback(castedUnit.AddSupportFollowUp);
            }
        }

        protected override void DisposePassiveInternally(GridPawnUnit unit)
        {
            if (unit is PvMnBattleGeneralUnit castedUnit)
            {
                _logicPreStartedEvent.UnregisterCallback(castedUnit.AddSupportFollowUp);
            }
        }
    }
    
    internal static class SupportAttackExtForUnit
    {
        internal static void AddSupportFollowUp(this PvMnBattleGeneralUnit ownerUnit, IEventOwner eventOwner,
            UnitAndAbilityEventParam usingParam)
        {
            var triggerUnit = usingParam.unit;
            if (TacticBattleManager.GetTeamAffinity(ownerUnit.GetTeam(), triggerUnit.GetTeam()) !=
                BattleTeam.Friendly || ownerUnit == triggerUnit)
            {
                return;
            }

            var triggerAbility = usingParam.ability;
            // The trigger player must use a FollowEnabled Attack
            // TODO: Consider support task
            if (!triggerAbility.IsFollowUpAllowed())
            {
                return;
            }

            var ability = ownerUnit.EquippedAbility;
            var results = usingParam.ResultsReference;

            var target = usingParam.target;

            List<LevelCellBase> reachableCells = ability.GetAbilityCells(ownerUnit);
            var bIsAbleToSupport =
                reachableCells.Count > 0 && reachableCells.Contains(target.GetCell());
            if (bIsAbleToSupport)
            {
                ability.HandleAbilityParam(ownerUnit, target, results);
            }
        }
    }
}