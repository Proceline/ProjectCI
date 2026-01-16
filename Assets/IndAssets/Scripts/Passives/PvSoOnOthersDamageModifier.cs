using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Passives
{
    
    [CreateAssetMenu(fileName = "PvSoOnOthersDamageModifier", menuName = "ProjectCI Passives/PvSoOnOthersDamageModifier", order = 1)]
    public class PvSoOnOthersDamageModifier : PvSoPassiveIndividual
    {
        [SerializeField]
        private PvSoNotifyDamageBeforeRevEvent onNotifyDamageBeforeRevEvent;

        [SerializeField]
        private PvSoUnitAndAbilityEvent onLogicFinishedEvent;

        [SerializeField]
        private PvSoSimpleDamageApplyEvent raiserSimpleDamageApplyEvent;

        [SerializeField]
        private int finalDamageValue;

        [SerializeField]
        private BattleTeam involvedTeam;

        [NonSerialized]
        private bool _isInstalled;

        [NonSerialized]
        private readonly List<PvMnBattleGeneralUnit> _ownersList = new();

        [NonSerialized]
        private readonly Queue<(GridPawnUnit, GridPawnUnit, int)> _pendingQuotes = new();

        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            if (!_isInstalled)
            {
                onNotifyDamageBeforeRevEvent.RegisterPostCallback(ModifyValueForOwner);
                onLogicFinishedEvent.RegisterCallback(AddLoadedHoldingDamage);
            }

            _isInstalled = true;
            _ownersList.Add(unit);
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            if (_isInstalled && OwnerCount == 1)
            {
                onNotifyDamageBeforeRevEvent.UnregisterPostCallback(ModifyValueForOwner);
                onLogicFinishedEvent.UnregisterCallback(AddLoadedHoldingDamage);
                _isInstalled = false;
            }

            _ownersList.Remove(unit);
        }

        private void ModifyValueForOwner(int[] allocatedValues, GridPawnUnit receiver, GridPawnUnit triggerOwner, uint extraInfo)
        {
            PvEnDamageForm damageForm = (PvEnDamageForm)extraInfo;
            if (damageForm.HasFlag(PvEnDamageForm.Support))
            {
                return;
            }

            var receivedCell = receiver.GetCell();
            bool validTarget = false;
            GridPawnUnit determinedOwner = null;

            foreach (var passiveOwner in _ownersList)
            {
                var difference = passiveOwner.GridPosition - receivedCell.GetIndex();
                var distance = Mathf.Abs(difference.x) + Mathf.Abs(difference.y);

                if (distance == 1)
                {
                    var teamRelation = TacticBattleManager.GetTeamAffinity(passiveOwner.GetTeam(), receiver.GetTeam());
                    if (teamRelation == involvedTeam)
                    {
                        var health = passiveOwner.RuntimeAttributes.Health;
                        if (health.CurrentValue >= health.MaxValue)
                        {
                            validTarget = true;
                            determinedOwner = passiveOwner;
                            break;
                        }
                    }
                }
            }

            if (!validTarget)
            {
                return;
            }

            var originalValue = allocatedValues[0];
            if (finalDamageValue >= originalValue)
            {
                return;
            }

            var delta = originalValue - finalDamageValue;
            _pendingQuotes.Enqueue((determinedOwner, triggerOwner, delta));
            allocatedValues[0] = finalDamageValue;
        }

        private void AddLoadedHoldingDamage(IEventOwner eventOwner, UnitAndAbilityEventParam usingParam)
        {
            while (_pendingQuotes.TryDequeue(out var quote))
            {
                var (pendingOwner, assultant, delta) = quote;
                if (pendingOwner)
                {
                    if (pendingOwner.IsDead())
                    {
                        continue;
                    }

                    PvSoDirectDamageAbilityParams.Execute(assultant, pendingOwner, usingParam.ResultsReference, delta, false, PvEnDamageType.None);
                }
            }
        }
    }
}