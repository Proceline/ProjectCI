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
        
        protected override void ReorderCombatingList(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<CombatingQueryContext> queryContexts)
        {
            if (!IsResponsiveOwner(inUnit, inTarget))
            {
                return;
            }
            
            AdjustQueryList(queryContexts);
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

        private void OnSingleCombatingQueryListReceived(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<CombatingQueryContext> queryContexts)
        {
            if (!IsOwner(inUnit.EventIdentifier))
            {
                return;
            }

            var query = queryContexts[0];
            if (query.QueryType == CombatingQueryType.ExtraFollowUp)
            {
                onEffectAppliedEvent.Invoke(inUnit);
            }
        }

        private void OnSingleCombatingQueryListLeft(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<CombatingQueryContext> queryContexts)
        {
            if (!IsOwner(inUnit.EventIdentifier))
            {
                return;
            }

            var query = queryContexts[0];
            if (query.QueryType != CombatingQueryType.ExtraFollowUp)
            {
                return;
            }

            onEffectDisposedEvent.Invoke(inUnit);

            OnCombatingQueryStartedEvent.UnregisterCallback(OnSingleCombatingQueryListReceived);
            OnCombatingQueryEndedEvent.UnregisterCallback(OnSingleCombatingQueryListLeft);
            
            Debug.Log($"Successfully Remove Effect from <{nameof(PvSoPassiveRelicEnforceFollowUp)}>");
        }
    }
}