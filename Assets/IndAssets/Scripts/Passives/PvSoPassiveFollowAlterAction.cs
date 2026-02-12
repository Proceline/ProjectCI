using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using System.Collections.Generic;
using UnityEngine;

namespace IndAssets.Scripts.Passives
{
    using QueryItem = PvAbilityQueryItem<PvMnBattleGeneralUnit>;

    /// <summary>
    /// Represents a passive action for INTP-type units that alters follow-up combat behavior by applying additional
    /// damage based on physical and magical resistance attributes.
    /// </summary>
    [CreateAssetMenu(fileName = "PvSoPassiveFollowAlterAction", menuName = "ProjectCI Passives/MBTI/INTP", order = 1)]
    public class PvSoPassiveFollowAlterAction : PvSoPassiveIndividual
    {
        [SerializeField]
        private PvSoUnitAbility physicAbility;

        [SerializeField]
        private PvSoUnitAbility magicAbility;

        [SerializeField]
        private AttributeType physResistAttribute;

        [SerializeField]
        private AttributeType magcResistAttribute;

        protected override void InstallPassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            PvSoPassiveFollowEncourage.OnCombatingListCreatedEvent.RegisterCallback(AdjustFollowUpWithAddition);
        }

        protected override void DisposePassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            PvSoPassiveFollowEncourage.OnCombatingListCreatedEvent.UnregisterCallback(AdjustFollowUpWithAddition);
        }

        protected override void InstallPassivePersonally(PvMnBattleGeneralUnit unit)
        {
            // Empty
        }

        protected override void DisposePassivePersonally(PvMnBattleGeneralUnit unit)
        {
            // Empty
        }

        private void AdjustFollowUpWithAddition(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget, List<QueryItem> queryItems)
        {
            if (!IsOwner(inUnit.ID))
            {
                return;
            }

            var physResist = inTarget.RuntimeAttributes.GetAttributeValue(physResistAttribute);
            var magcResist = inTarget.RuntimeAttributes.GetAttributeValue(magcResistAttribute);

            var followUpIndex = queryItems.FindIndex
                (query => query.queryOrderForm.HasFlag(PvEnDamageForm.FollowUp)
                && !query.queryOrderForm.HasFlag(PvEnDamageForm.Counter));

            if (followUpIndex < 0 || !queryItems[followUpIndex].enabled || inUnit.IsDead())
            {
                return;
            }

            var insertAt = followUpIndex + 1;
            var queryItem = QueryItem.CreateQueryItemIntoList(queryItems, insertAt);
            queryItem.holdingOwner = inUnit;
            queryItem.targetUnit = inTarget;
            queryItem.SetAbility(physResist > magcResist ? magicAbility : physicAbility, PvEnDamageForm.Aggressive);
        }
    }
}