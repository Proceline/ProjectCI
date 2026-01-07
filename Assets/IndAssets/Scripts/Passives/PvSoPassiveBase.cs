using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Passives
{
    public enum PvEnPassiveTiming
    {
        None,
        Initialized
    }
    
    public abstract class PvSoPassiveBase : ScriptableObject
    {
        public PvEnPassiveTiming passiveTiming;
        
        [SerializeField] private string passiveName;
        public string PassiveName => passiveName;
        public string description;

        [NonSerialized] private readonly HashSet<string> _recordedOwners = new();
        public void InstallPassive(PvMnBattleGeneralUnit unit)
        {
            if (!_recordedOwners.Add(unit.ID))
            {
                Debug.LogWarning($"{unit.name} already registered this passive!");
                return;
            }

            InstallPassiveInternally(unit);
        }

        public void DisposePassive(PvMnBattleGeneralUnit unit)
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