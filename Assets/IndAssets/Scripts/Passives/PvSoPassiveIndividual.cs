using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
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

        protected abstract void InstallPassiveGenerally(PvMnBattleGeneralUnit unit);
        protected abstract void DisposePassiveGenerally(PvMnBattleGeneralUnit unit);
        protected abstract void InstallPassivePersonally(PvMnBattleGeneralUnit unit);
        protected abstract void DisposePassivePersonally(PvMnBattleGeneralUnit unit);
    }
}