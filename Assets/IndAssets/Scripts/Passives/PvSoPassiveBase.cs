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

        public abstract void InstallPassive(PvMnBattleGeneralUnit unit);
        public abstract void DisposePassive(PvMnBattleGeneralUnit unit);
    }
}