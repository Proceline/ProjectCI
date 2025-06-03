namespace ProjectCI.CoreSystem.Runtime.Abilities.Enums
{
    public enum InitiativeType
    {
        None,
        Initiative,
        Counter,
        FollowUp
    }

    public struct CombatActionContext
    {
        public bool IsVictim;
        public InitiativeType InitiativeType;
    }

    public enum FollowUpCondition
    {
        None,
        InitiativeFollowUp,
        CounterFollowUp
    }
}