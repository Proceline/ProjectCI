using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Relics
{
    [CreateAssetMenu(fileName = "SoRelic_HurryFollowUp", menuName = "ProjectCI Passives/Relics/HurryFollowUp", order = 1)]
    public class PvSoPassiveRelicHurryFollowUp : PvSoPassiveRelicHurryCounter
    {
        protected override bool IsResponsiveOwner(PvMnBattleGeneralUnit caster, PvMnBattleGeneralUnit victim)
        {
            return IsOwner(caster.EventIdentifier);
        }

        protected override bool IsSpeedCheckPassed(int casterSpeed, int victimSpeed)
        {
            var delta = casterSpeed - victimSpeed;
            return delta >= triggerDeltaValue;
        }

        protected override void AdjustQueryList(List<CombatingQueryContext> contextQueryList)
        {
            var index = contextQueryList.FindIndex(query =>
                query is { IsCounter: false, QueryType: CombatingQueryType.AutoFollowUp });
            
            // If FollowUp already at INDEX(1), means it must be already right after FirstAttempt
            // Otherwise, if index==-1, then there is no followUp
            if (index < 2)
            {
                return;
            }
            
            var attemptIndex = contextQueryList.FindIndex(query =>
                query is { IsCounter: false, QueryType: CombatingQueryType.FirstAttempt });

            var firstCounterQuery = contextQueryList[index];
            contextQueryList.RemoveAt(index);
            contextQueryList.Insert(attemptIndex + 1, firstCounterQuery);
        }
    }
}