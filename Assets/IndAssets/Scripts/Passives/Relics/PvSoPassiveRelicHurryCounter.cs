using System.Collections.Generic;
using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace IndAssets.Scripts.Passives.Relics
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "SoRelic_HurryCounter", menuName = "ProjectCI Passives/Relics/HurryCounter", order = 1)]
    public class PvSoPassiveRelicHurryCounter : PvSoPassiveRelic
    {
        [FormerlySerializedAs("speedAttributeType")] 
        [SerializeField] protected AttributeType targetAttributeType;
        [SerializeField] protected int triggerDeltaValue;
        
        [Inject] private static readonly IUnitGeneralCombatingEvent OnCombatingListCreatedEvent;
        
        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            OnCombatingListCreatedEvent.RegisterCallback(ReorderCombatingList);
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            OnCombatingListCreatedEvent.UnregisterCallback(ReorderCombatingList);
        }

        protected virtual void ReorderCombatingList(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<PvAbilityQueryItem<PvMnBattleGeneralUnit>> queryItems)
        {
            if (!IsResponsiveOwner(inUnit, inTarget))
            {
                return;
            }

            var casterSpeed = inUnit.RuntimeAttributes.GetAttributeValue(targetAttributeType);
            var victimSpeed = inTarget.RuntimeAttributes.GetAttributeValue(targetAttributeType);

            if (IsAttributeCheckPassed(casterSpeed, victimSpeed))
            {
                AdjustQueryList(queryItems);
            }
        }

        protected virtual bool IsResponsiveOwner(PvMnBattleGeneralUnit caster, PvMnBattleGeneralUnit victim)
        {
            return IsOwner(victim.EventIdentifier);
        }
        
        protected virtual bool IsAttributeCheckPassed(int casterSpeed, int victimSpeed)
        {
            var delta = victimSpeed - casterSpeed;
            return delta >= triggerDeltaValue;
        }

        private void AdjustQueryList(List<PvAbilityQueryItem<PvMnBattleGeneralUnit>> queryItems)
        {
            var index = queryItems.FindIndex(query =>
            {
                return query.queryOrderForm.HasFlag(PvEnDamageForm.Counter);
            });

            if (index <= 0 || !queryItems[index].enabled)
            {
                return;
            }

            var firstCounterQuery = queryItems[index];
            queryItems.RemoveAt(index);
            queryItems.Insert(0, firstCounterQuery);
        }
    }
}