using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
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
        
        protected override void InstallPassiveInternally(GridPawnUnit unit)
        {
            OnCombatingListCreatedEvent.RegisterCallback(ReorderCombatingList);
        }

        protected override void DisposePassiveInternally(GridPawnUnit unit)
        {
            OnCombatingListCreatedEvent.UnregisterCallback(ReorderCombatingList);
        }

        protected virtual void ReorderCombatingList(IEventOwner raiser, UnitCombatingEventParam combatingParam)
        {
            var list = combatingParam.CombatingList;
            var caster = combatingParam.unit;
            var victim = combatingParam.target;
            if (!IsResponsiveOwner(caster, victim))
            {
                return;
            }

            var casterSpeed = caster.RuntimeAttributes.GetAttributeValue(targetAttributeType);
            var victimSpeed = victim.RuntimeAttributes.GetAttributeValue(targetAttributeType);

            if (IsAttributeCheckPassed(casterSpeed, victimSpeed))
            {
                AdjustQueryList(list);
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

        protected virtual void AdjustQueryList(List<CombatingQueryContext> contextQueryList)
        {
            var index = contextQueryList.FindIndex(query =>
                query is { IsCounter: true, QueryType: CombatingQueryType.FirstAttempt });
            if (index <= 0)
            {
                return;
            }

            var firstCounterQuery = contextQueryList[index];
            contextQueryList.RemoveAt(index);
            contextQueryList.Insert(0, firstCounterQuery);
        }
    }
}