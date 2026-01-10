using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Passives
{   
    public abstract class PvSoPassiveIndividual : PvSoPassiveBase
    {
        [NonSerialized] private readonly HashSet<string> _recordedOwners = new();
        
        public override void InstallPassive(PvMnBattleGeneralUnit unit)
        {
            if (!_recordedOwners.Add(unit.ID))
            {
                Debug.LogWarning($"{unit.name} already registered this passive!");
                return;
            }

            InstallPassiveInternally(unit);
        }

        public override void DisposePassive(PvMnBattleGeneralUnit unit)
        {
            if (IsOwner(unit.ID))
            {
                DisposePassiveInternally(unit);
                _recordedOwners.Remove(unit.ID);
                return;
            }
            Debug.LogWarning($"{unit.name} hasn't registered this passive!");
        }

        protected bool IsOwner(string unitId) => _recordedOwners.Contains(unitId);

        protected abstract void InstallPassiveInternally(PvMnBattleGeneralUnit unit);
        protected abstract void DisposePassiveInternally(PvMnBattleGeneralUnit unit);
    }
}