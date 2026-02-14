using IndAssets.Scripts.Abilities;
using IndAssets.Scripts.Passives;
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
    using QueryItem = PvAbilityQueryItem<PvMnBattleGeneralUnit>;

    [CreateAssetMenu(fileName = "PvSoOnOthersDamageModifier", menuName = "ProjectCI Passives/PvSoOnOthersDamageModifier", order = 1)]
    public class PvSoOnOthersDamageModifier : PvSoPassiveIndividual
    {
        [SerializeField]
        private PvSoNotifyDamageBeforeRevEvent onNotifyDamageBeforeRevEvent;

        [SerializeField]
        private PvSoSimpleDamageApplyEvent raiserSimpleDamageApplyEvent;

        [SerializeField]
        private int finalDamageValue;

        private readonly Dictionary<string, string> _ownersQueryIds = new();

        [SerializeField]
        private PvSoDynamicDamageAbilityParams damageParams;

        [SerializeField]
        private PvSoUnitAbility pureDamageAbility;

        [SerializeField]
        private PvEnDamageType damageType;

        protected override void InstallPassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            PvSoPassiveFollowEncourage.OnCombatingListFinishedEvent.RegisterCallback(AdjustAfterFinishedEvent);
            onNotifyDamageBeforeRevEvent.RegisterPostCallback(ModifyValueForOwner);
        }

        protected override void DisposePassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            PvSoPassiveFollowEncourage.OnCombatingListFinishedEvent.UnregisterCallback(AdjustAfterFinishedEvent);
            onNotifyDamageBeforeRevEvent.UnregisterPostCallback(ModifyValueForOwner);
        }

        protected override void InstallPassivePersonally(PvMnBattleGeneralUnit unit)
        {
            // Empty
        }

        protected override void DisposePassivePersonally(PvMnBattleGeneralUnit unit)
        {
            _ownersQueryIds.Remove(unit.ID);
        }

        private void AdjustAfterFinishedEvent(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget, List<QueryItem> queryItems)
        {
#if UNITY_EDITOR
            if (damageParams && pureDamageAbility)
            {
                if (pureDamageAbility.GetParameters()[0] != damageParams)
                {
                    Debug.LogError("The pure damage ability's parameters do not match the specified damage parameters.");
                    return;
                }
            }
            else
            {
                throw new Exception("ERROR: Damage parameters or pure damage ability is not set. Please ensure both are assigned in the inspector.");
            }
#else
            if (damageParams && pureDamageAbility)
            {
                var parameters = pureDamageAbility.GetParameters();
                if (parameters.Count == 0 || !parameters.Contains(damageParams))
                {
                    parameters.Clear();
                    parameters.Add(damageParams);
                }
            }
            else
            {
                throw new Exception("ERROR: Damage parameters or pure damage ability is not set. Please ensure both are assigned in the inspector.");
            }

#endif

            foreach (var owner in OwnersList)
            {
                var queryItem = QueryItem.CreateQueryItemIntoList(queryItems);
                queryItem.SetAbility(pureDamageAbility, PvEnDamageForm.Aggressive);
                queryItem.queryOrderForm |= PvEnDamageForm.Additional;
                queryItem.holdingOwner = owner;
                queryItem.targetUnit = owner;
                _ownersQueryIds[owner.ID] = queryItem.UniqueId;
            }
        }

        /// <summary>
        /// Only applied while damage is not mocked
        /// </summary>
        /// <param name="allocatedValues"></param>
        /// <param name="resultId">ResultId can be Empty, during Mock value process</param>
        /// <param name="receiver"></param>
        /// <param name="triggerOwner"></param>
        /// <param name="extraInfo"></param>
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

            foreach (var passiveOwner in OwnersList)
            {
                var difference = passiveOwner.GridPosition - receivedCell.GetIndex();
                var distance = Mathf.Abs(difference.x) + Mathf.Abs(difference.y);

                if (distance == 1)
                {
                    var teamRelation = TacticBattleManager.GetTeamAffinity(passiveOwner.GetTeam(), receiver.GetTeam());
                    if (teamRelation == BattleTeam.Friendly)
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
            allocatedValues[0] = finalDamageValue;

            if (_ownersQueryIds.TryGetValue(determinedOwner.ID, out var resultId))
            {
                damageParams.SetupDynamicDamage(resultId, delta, damageType);
            }
        }
    }
}