using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using System.Collections.Generic;

namespace ProjectCI.CoreSystem.Runtime.Passives
{
    public abstract class PvSoPassiveIndividual : PvSoPassiveBase
    {
        protected readonly List<PvMnBattleGeneralUnit> OwnersList = new();
        private readonly HashSet<string> _ownersIdSet = new();
        protected int OwnerCount => OwnersList.Count;

        public override void InstallPassive(PvMnBattleGeneralUnit unit)
        {
            if (OwnersList.Count == 0)
            {
                InstallPassiveGenerally(unit);
            }

            if (!OwnersList.Contains(unit))
            {
                InstallPassivePersonally(unit);
                OwnersList.Add(unit);
                _ownersIdSet.Add(unit.ID);
            }
        }

        public override void DisposePassive(PvMnBattleGeneralUnit unit)
        {
            if (OwnersList.Count == 1)
            {
                DisposePassiveGenerally(unit);
            }

            if (OwnersList.Remove(unit))
            {
                _ownersIdSet.Remove(unit.ID);
                DisposePassivePersonally(unit);
            }
        }

        protected bool IsOwner(string unitId)
        {
            return _ownersIdSet.Contains(unitId);
        }

        /// <summary>
        /// Only used when the buff need to be assigned while there is no Buff Owner
        /// </summary>
        /// <param name="unit"></param>
        protected abstract void InstallPassiveGenerally(PvMnBattleGeneralUnit unit);

        /// <summary>
        /// Only used when the dispose target is the last Buff Owner
        /// </summary>
        /// <param name="unit"></param>
        protected abstract void DisposePassiveGenerally(PvMnBattleGeneralUnit unit);

        /// <summary>
        /// Used when each unit needs specific register
        /// </summary>
        /// <param name="unit"></param>
        protected abstract void InstallPassivePersonally(PvMnBattleGeneralUnit unit);

        /// <summary>
        /// Used when each unit needs specific exit function while unregister
        /// </summary>
        /// <param name="unit"></param>
        protected abstract void DisposePassivePersonally(PvMnBattleGeneralUnit unit);

        public override void ClearPassivesWhileRoundStarted(BattleTeam battleTeam)
        {
            if (Duration == PvPassiveDuration.Infinite)
            {
                return;
            }

#if UNITY_EDITOR
            UnityEngine.Debug.Log($">>>>>>Try to clean up Passive[{name}]...");
#endif

            for (var i = OwnersList.Count - 1; i >= 0; i--)
            {
                var unit = OwnersList[i];

                if ((unit.GetTeam() != battleTeam && Duration == PvPassiveDuration.EndInHostile) ||
                    (unit.GetTeam() == battleTeam && Duration == PvPassiveDuration.EndInFriendly))
                {
                    DisposePassive(unit);
#if UNITY_EDITOR
                    UnityEngine.Debug.Log($"Passive[{name}] on Unit[{unit.name}] has been Disposed!");
#endif
                }
            }
        }
    }
}