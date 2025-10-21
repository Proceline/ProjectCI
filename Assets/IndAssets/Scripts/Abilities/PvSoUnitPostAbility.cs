using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "NewPostAbility", menuName = "ProjectCI Tools/Ability/Create Custom Post Ability", order = 1)]
    public class PvSoUnitPostAbility : PvSoUnitAbility
    {
        [SerializeField] private bool isFullPost;
        
        public override List<CombatingQueryContext> OnCombatingQueryListCreated(PvMnBattleGeneralUnit caster,
            PvMnBattleGeneralUnit victim, bool casterSpeedExceed, bool victimSpeedExceed)
        {
            var baseResults =
                base.OnCombatingQueryListCreated(caster, victim, casterSpeedExceed, victimSpeedExceed);

            var resultsCount = baseResults.Count;
            if (resultsCount <= 1)
            {
                return baseResults;
            }

            var initiativeQuery = baseResults[0];
            var counterQuery = baseResults[1];
            baseResults[0] = counterQuery;
            baseResults[1] = initiativeQuery;

            if (isFullPost && resultsCount > 2)
            {
                var toReorderResults = new Queue<CombatingQueryContext>();
                for (var i = resultsCount - 1; i >= 2; i--)
                {
                    toReorderResults.Enqueue(baseResults[i]);
                    baseResults.RemoveAt(i);
                }

                while (toReorderResults.TryDequeue(out var result))
                {
                    baseResults.Insert(1, result);
                }
            }

            return baseResults;
        }
    }
}