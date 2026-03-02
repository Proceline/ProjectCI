using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Passives
{
    public enum PvPassiveDuration
    {
        Infinite = 0,
        EndInHostile = 1,
        EndInFriendly = 2
    }

    public abstract class PvSoPassiveBase : ScriptableObject
    {
        [SerializeField] private PvPassiveDuration duration;
        [SerializeField] private string passiveName;

        protected PvPassiveDuration Duration => duration;
        public string PassiveName => passiveName;
        public string description;

        public abstract void InstallPassive(PvMnBattleGeneralUnit unit);
        public abstract void DisposePassive(PvMnBattleGeneralUnit unit);

        /// <summary>
        /// Bind to Round Start Event if this need to be clear on Round Start
        /// </summary>
        /// <param name="battleTeam"></param>
        public abstract void ClearPassivesWhileRoundStarted(BattleTeam battleTeam);
    }
}