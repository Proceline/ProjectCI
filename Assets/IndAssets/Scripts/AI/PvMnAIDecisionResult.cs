using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace IndAssets.Scripts.AI
{
    /// <summary>
    /// Result of AI decision making, containing movement and attack information
    /// </summary>
    public class PvMnAIDecisionResult
    {
        /// <summary>
        /// Target cell to move to. Null if no movement needed
        /// </summary>
        public LevelCellBase MoveToCell { get; set; }

        /// <summary>
        /// Target cell to attack. Null if no attack action
        /// </summary>
        public LevelCellBase AttackTargetCell { get; set; }

        /// <summary>
        /// Ability to use for attack. Null if using default EquippedAbility
        /// </summary>
        public PvSoUnitAbility AbilityToUse { get; set; }

        /// <summary>
        /// Whether this unit should take rest (skip turn)
        /// </summary>
        public bool ShouldTakeRest { get; set; }

        public bool HasAction => !ShouldTakeRest && (MoveToCell != null || AttackTargetCell != null);
    }
}

