using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities
{
    [CreateAssetMenu(fileName = "NewPostAbility", menuName = "ProjectCI Tools/Ability/Create Custom Post Ability", order = 1)]
    public class PvSoUnitPostAbility : PvSoUnitAbility
    {
        public override List<CombatingQueryContext> OnCombatingQueryListCreated(PvMnBattleGeneralUnit caster,
            PvMnBattleGeneralUnit victim, bool casterSpeedExceed, bool victimSpeedExceed)
        {
            var baseResult =
                base.OnCombatingQueryListCreated(caster, victim, casterSpeedExceed, victimSpeedExceed);

            if (baseResult.Count <= 1)
            {
                return baseResult;
            }

            var initiativeQuery = baseResult[0];
            var counterQuery = baseResult[1];
            baseResult[0] = counterQuery;
            baseResult[1] = initiativeQuery;

            return baseResult;
        }
    }
}