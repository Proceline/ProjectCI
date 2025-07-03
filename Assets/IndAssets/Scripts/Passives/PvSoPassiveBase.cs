using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
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

        [NonSerialized] private readonly HashSet<string> _recordedOwners = new();
        public void InstallPassive(GridPawnUnit unit)
        {
            if (!_recordedOwners.Add(unit.ID))
            {
                Debug.LogWarning($"{unit.name} already registered this passive!");
                return;
            }

            InstallPassiveInternally(unit);
        }

        public void DisposePassive(GridPawnUnit unit)
        {
            if (_recordedOwners.Contains(unit.ID))
            {
                DisposePassiveInternally(unit);
                _recordedOwners.Remove(unit.ID);
                return;
            }
            Debug.LogWarning($"{unit.name} hasn't registered this passive!");
        }

        protected abstract void InstallPassiveInternally(GridPawnUnit unit);
        protected abstract void DisposePassiveInternally(GridPawnUnit unit);
    }
}