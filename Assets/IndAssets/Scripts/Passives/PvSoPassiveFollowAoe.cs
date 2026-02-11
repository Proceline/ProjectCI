using IndAssets.Scripts.Abilities;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Passives;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using System.Collections.Generic;
using UnityEngine;

namespace IndAssets.Scripts.Passives
{
    using QueryItem = PvAbilityQueryItem<PvMnBattleGeneralUnit>;

    /// <summary>
    /// Sample passive of ESFP
    /// Encourages the owner to be in the middle of the battle by applying support ability on adjacent friendly units when doing follow-up attacks.
    /// </summary>
    [CreateAssetMenu(fileName = "PvSoPassiveFollowAoe", menuName = "ProjectCI Passives/PvSoPassiveFollowAoe", order = 1)]
    public class PvSoPassiveFollowAoe : PvSoPassiveIndividual
    {
        [SerializeField]
        private PvSoUnitAbility followUpAbility;

        [SerializeField]
        private bool targetToSelf = false;

        protected override void InstallPassiveInternally(PvMnBattleGeneralUnit unit)
        {
            PvSoPassiveFollowEncourage.OnCombatingListCreatedEvent.RegisterCallback(AdjustFollowUpToAoe);
        }

        protected override void DisposePassiveInternally(PvMnBattleGeneralUnit unit)
        {
            PvSoPassiveFollowEncourage.OnCombatingListCreatedEvent.UnregisterCallback(AdjustFollowUpToAoe);
        }

        private void AdjustFollowUpToAoe(PvMnBattleGeneralUnit inUnit, PvMnBattleGeneralUnit inTarget,
            List<QueryItem> queryItems)
        {
            if (!IsOwner(inUnit.ID))
            {
                return;
            }

            var neighbors = inUnit.GetCell().GetAllAdjacentCells();
            var friendlyNeighborsCount = 0;
            foreach (var neighborCell in neighbors)
            {
                if (TacticBattleManager.GetTeamAffinity(inUnit.GetTeam(), neighborCell.GetCellTeam()) 
                    == BattleTeam.Friendly)
                {
                    friendlyNeighborsCount++;
                }
            }
            if (friendlyNeighborsCount < 2)
            {
                return;
            }

            var followUpIndex = queryItems.FindIndex
                (query => query.queryOrderForm.HasFlag(PvEnDamageForm.FollowUp)
                && !query.queryOrderForm.HasFlag(PvEnDamageForm.Counter));

            if (followUpIndex < 0 || !queryItems[followUpIndex].enabled || inUnit.IsDead())
            {
                return;
            }

            var queryItem = queryItems[followUpIndex];
            queryItem.SetAbility(followUpAbility, PvEnDamageForm.Aggressive);
            if (targetToSelf)
            {
                queryItem.targetUnit = inUnit;
            }
        }
    }
}