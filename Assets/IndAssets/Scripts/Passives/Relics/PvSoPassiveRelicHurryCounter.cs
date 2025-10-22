using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;

namespace IndAssets.Scripts.Passives.Relics
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "SoRelic_HurryCounter", menuName = "ProjectCI Passives/Relics/HurryCounter", order = 1)]
    public class PvSoPassiveRelicHurryCounter : PvSoPassiveRelic
    {
        [SerializeField] private AttributeType speedAttributeType;
        [SerializeField] private int triggerDeltaValue;
        
        [Inject] private static readonly IUnitGeneralCombatingEvent OnCombatingListCreatedEvent;
        
        protected override void InstallPassiveInternally(GridPawnUnit unit)
        {
            OnCombatingListCreatedEvent.RegisterCallback(ReorderCombatingList);
        }

        protected override void DisposePassiveInternally(GridPawnUnit unit)
        {
            OnCombatingListCreatedEvent.UnregisterCallback(ReorderCombatingList);
        }

        private void ReorderCombatingList(IEventOwner raiser, UnitCombatingEventParam combatingParam)
        {
            var list = combatingParam.CombatingList;
            var caster = combatingParam.unit;
            var victim = combatingParam.target;
            if (!IsOwner(victim.EventIdentifier))
            {
                return;
            }

            var casterSpeed = caster.RuntimeAttributes.GetAttributeValue(speedAttributeType);
            var victimSpeed = victim.RuntimeAttributes.GetAttributeValue(speedAttributeType);

            var delta = victimSpeed - casterSpeed;
            if (delta >= triggerDeltaValue)
            {
                MoveCounterToFront(list);
            }
        }

        private void MoveCounterToFront(List<CombatingQueryContext> contextQueryList)
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