using IndAssets.Scripts.Abilities;
using IndAssets.Scripts.Events;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using System.Collections.Generic;
using UnityEngine;

namespace IndAssets.Scripts.Passives
{
    [CreateAssetMenu(fileName = "PvSoPassiveFollowAlterAction", menuName = "ProjectCI Passives/MBTI/INTP", order = 1)]
    public class PvSoPassiveFollowAlterAction : PvSoPassiveIndividual
    {
        [SerializeField]
        PvEnDamageType physDamageType;

        [SerializeField]
        PvEnDamageType magcDamageType;

        [SerializeField]
        private AttributeType physResistAttribute;

        [SerializeField]
        private AttributeType magcResistAttribute;

        [SerializeField]
        private int extraDamage;

        private CombatingQueryContext _additionalMark = new CombatingQueryContext
        {
            IsCounter = false,
            QueryType = CombatingQueryType.Additional
        };

        private readonly Dictionary<string, (PvEnDamageType, int)> _chosenDamage = new();

        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            PvSoPassiveFollowEncourage.OnCombatingListCreatedEvent.RegisterCallback(AdjustFollowUpWithAddition);
            unit.RegisterQueryApply(_additionalMark, ApplyAdditionalOnVictim, true);
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            PvSoPassiveFollowEncourage.OnCombatingListCreatedEvent.UnregisterCallback(AdjustFollowUpWithAddition);
            unit.UnregisterQueryApply(_additionalMark, ApplyAdditionalOnVictim);
        }

        private void AdjustFollowUpWithAddition(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<CombatingQueryContext> queryContexts)
        {
            if (!IsOwner(inUnit.ID))
            {
                return;
            }

            if (inTarget.IsDead())
            {
                return;
            }

            var physResist = inTarget.RuntimeAttributes.GetAttributeValue(physResistAttribute);
            var magcResist = inTarget.RuntimeAttributes.GetAttributeValue(magcResistAttribute);

            var possiblePhysDamage = extraDamage - physResist;
            var possibleMagcDamage = extraDamage - magcResist;

            var finalChoice = possiblePhysDamage >= possibleMagcDamage? physDamageType : magcDamageType;
            var possibleDamage = finalChoice == physDamageType ? possiblePhysDamage : possibleMagcDamage;
            var finalExtraDamage = extraDamage;
            if (possibleDamage <= 0)
            {
                finalExtraDamage = 1 + (physResist > magcResist ? physResist : magcResist);
            }

            var followUpIndex = queryContexts.FindIndex(query => query.QueryType == CombatingQueryType.AutoFollowUp);
            if (followUpIndex < 0)
            {
                return;
            }

            var insertAt = followUpIndex + 1;
            var addQuery = queryContexts[followUpIndex];
            addQuery.QueryType = CombatingQueryType.Additional;
            queryContexts.Insert(insertAt, addQuery);
            _chosenDamage.Add(inUnit.ID, (finalChoice, finalExtraDamage));
        }

        private void ApplyAdditionalOnVictim(PvMnBattleGeneralUnit fromUnit, PvMnBattleGeneralUnit toUnit, Queue<CommandResult> commands)
        {
            if (!_chosenDamage.TryGetValue(fromUnit.ID, out var chosenDamagePair))
            {
                chosenDamagePair = (PvEnDamageType.None, extraDamage);
            }

            PvSoDirectDamageAbilityParams.Execute(fromUnit, toUnit, commands, chosenDamagePair.Item2, false, chosenDamagePair.Item1);
            _chosenDamage.Remove(fromUnit.ID);
        }
    }
}