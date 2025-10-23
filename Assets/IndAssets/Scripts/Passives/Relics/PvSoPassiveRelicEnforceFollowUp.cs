using System.Collections.Generic;
using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;

namespace IndAssets.Scripts.Passives.Relics
{
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "SoRelic_EnforceFollowUp", menuName = "ProjectCI Passives/Relics/EnforceFollowUp", order = 1)]
    public class PvSoPassiveRelicEnforceFollowUp : PvSoPassiveRelicHurryCounter
    {
        [Inject] private static readonly IUnitCombatingQueryStartEvent OnCombatingQueryStartedEvent;
        [Inject] private static readonly IUnitCombatingQueryEndEvent OnCombatingQueryEndedEvent;
        
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onEffectAppliedEvent;
        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onEffectDisposedEvent;
        
        protected override void ReorderCombatingList(IEventOwner raiser, UnitCombatingEventParam combatingParam)
        {
            var list = combatingParam.CombatingList;
            var caster = combatingParam.unit;
            var victim = combatingParam.target;
            if (!IsResponsiveOwner(caster, victim))
            {
                return;
            }
            
            AdjustQueryList(list);
        }
        
        protected override bool IsResponsiveOwner(PvMnBattleGeneralUnit caster, PvMnBattleGeneralUnit victim)
        {
            return IsOwner(caster.EventIdentifier);
        }

        protected override void AdjustQueryList(List<CombatingQueryContext> contextQueryList)
        {
            var followUpTriggered = contextQueryList.Exists(query =>
                query is { IsCounter: false, QueryType: CombatingQueryType.AutoFollowUp });
            
            if (!followUpTriggered)
            {
                return;
            }
            
            OnCombatingQueryStartedEvent.RegisterCallback(OnSingleCombatingQueryListReceived);
            OnCombatingQueryEndedEvent.RegisterCallback(OnSingleCombatingQueryListLeft);
            contextQueryList.Add(new CombatingQueryContext
                { IsCounter = false, QueryType = CombatingQueryType.ExtraFollowUp });
            
            Debug.Log($"Successfully Load Effect from <{nameof(PvSoPassiveRelicEnforceFollowUp)}>");
        }

        private void OnSingleCombatingQueryListReceived(IEventOwner raiser, UnitCombatingEventParam combatingParam)
        {
            var caster = combatingParam.unit;
            if (!IsOwner(caster.EventIdentifier))
            {
                return;
            }

            var query = combatingParam.CombatingList[0];
            if (query.QueryType == CombatingQueryType.ExtraFollowUp)
            {
                onEffectAppliedEvent.Invoke(caster);
            }
        }

        private void OnSingleCombatingQueryListLeft(IEventOwner raiser, UnitCombatingEventParam combatingParam)
        {
            var caster = combatingParam.unit;
            if (!IsOwner(caster.EventIdentifier))
            {
                return;
            }

            var query = combatingParam.CombatingList[0];
            if (query.QueryType != CombatingQueryType.ExtraFollowUp)
            {
                return;
            }

            onEffectDisposedEvent.Invoke(caster);

            OnCombatingQueryStartedEvent.UnregisterCallback(OnSingleCombatingQueryListReceived);
            OnCombatingQueryEndedEvent.UnregisterCallback(OnSingleCombatingQueryListLeft);
            
            Debug.Log($"Successfully Remove Effect from <{nameof(PvSoPassiveRelicEnforceFollowUp)}>");
        }
    }
}