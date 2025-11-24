using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace IndAssets.Scripts.AI
{
    /// <summary>
    /// Strategy for movement behavior
    /// </summary>
    public enum PvMnAIMovementStrategy
    {
        Aggressive,    // Always move towards enemies (charge type)
        Conservative,  // Only move when enemies are in attackable range (conservative type)
        Defensive      // Only move when enemies enter a set area range (defensive type)
    }

    /// <summary>
    /// Helper class for movement strategies
    /// </summary>
    public static class PvMnAIMovementHelper
    {
        /// <summary>
        /// Determine if unit should move based on strategy
        /// </summary>
        public static bool ShouldMove(
            PvMnBattleGeneralUnit aiUnit,
            PvMnAIMovementStrategy strategy,
            ICollection<LevelCellBase> availableMoveCells,
            ICollection<LevelCellBase> validTargets,
            Dictionary<LevelCellBase, List<LevelCellBase>> victimsFromCells,
            int defensiveRange = 5)
        {
            switch (strategy)
            {
                case PvMnAIMovementStrategy.Aggressive:
                    // Always try to move towards enemies if there are any
                    return validTargets is { Count: > 0 };

                case PvMnAIMovementStrategy.Conservative:
                    // Only move if we can attack from current position or after moving
                    var currentCell = aiUnit.GetCell();
                    var canAttackFromCurrent = CanAttackFromCell(currentCell, victimsFromCells);
                    
                    var canAttackAfterMove = false;

                    if (availableMoveCells != null)
                    {
                        foreach (var cell in availableMoveCells)
                        {
                            if (!CanAttackFromCell(cell, victimsFromCells)) continue;
                            canAttackAfterMove = true;
                            break;
                        }
                    }
                    
                    return !canAttackFromCurrent && canAttackAfterMove;

                case PvMnAIMovementStrategy.Defensive:
                    // Only move if enemies are within defensive range
                    return IsEnemyInRange(aiUnit, defensiveRange) && 
                           (validTargets == null || validTargets.Count == 0);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Find best movement cell based on strategy and target
        /// </summary>
        public static LevelCellBase FindBestMovementCell(
            PvMnBattleGeneralUnit aiUnit,
            PvMnAIMovementStrategy strategy,
            ICollection<LevelCellBase> availableMoveCells,
            LevelCellBase targetCell,
            Dictionary<LevelCellBase, List<LevelCellBase>> victimsFromCells)
        {
            if (availableMoveCells == null || availableMoveCells.Count == 0)
                return null;

            var aiCurrentCell = aiUnit.GetCell();

            switch (strategy)
            {
                case PvMnAIMovementStrategy.Aggressive:
                    // Move closest to target while being able to attack
                    if (targetCell != null && victimsFromCells != null)
                    {
                        LevelCellBase bestCell = null;
                        float minDistance = float.MaxValue;

                        foreach (var moveCell in availableMoveCells)
                        {
                            // Check if we can attack target from this position
                            if (victimsFromCells.TryGetValue(targetCell, out var attackPositions) &&
                                attackPositions.Contains(moveCell))
                            {
                                float dist = GetDistance(moveCell, targetCell);
                                if (dist < minDistance)
                                {
                                    minDistance = dist;
                                    bestCell = moveCell;
                                }
                            }
                        }

                        // If no cell can attack target, move closest to target
                        if (bestCell == null)
                        {
                            foreach (var moveCell in availableMoveCells)
                            {
                                float dist = GetDistance(moveCell, targetCell);
                                if (dist < minDistance)
                                {
                                    minDistance = dist;
                                    bestCell = moveCell;
                                }
                            }
                        }

                        return bestCell;
                    }
                    break;

                case PvMnAIMovementStrategy.Conservative:
                case PvMnAIMovementStrategy.Defensive:
                    // Move to position that can attack target, or stay close to current position
                    if (targetCell != null && victimsFromCells != null)
                    {
                        foreach (var moveCell in availableMoveCells)
                        {
                            if (victimsFromCells.TryGetValue(targetCell, out var attackPositions) &&
                                attackPositions.Contains(moveCell))
                            {
                                return moveCell;
                            }
                        }
                    }
                    // If can't attack, stay close to current position
                    return FindClosestCell(availableMoveCells, aiCurrentCell);
            }

            return null;
        }

        private static bool CanAttackFromCell(LevelCellBase cell, Dictionary<LevelCellBase, List<LevelCellBase>> victimsFromCells)
        {
            if (victimsFromCells == null) return false;
            foreach (var kvp in victimsFromCells)
            {
                if (kvp.Value.Contains(cell))
                    return true;
            }
            return false;
        }

        private static bool IsEnemyInRange(PvMnBattleGeneralUnit aiUnit, int range)
        {
            // TODO: Implement enemy detection in range
            // This should check if any enemy unit is within the specified range
            // For now, return false as placeholder
            return false;
        }

        private static LevelCellBase FindClosestCell(ICollection<LevelCellBase> cells, LevelCellBase target)
        {
            LevelCellBase closest = null;
            float minDist = float.MaxValue;

            foreach (var cell in cells)
            {
                float dist = GetDistance(cell, target);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = cell;
                }
            }

            return closest;
        }

        private static float GetDistance(LevelCellBase from, LevelCellBase to)
        {
            var fromPos = from.GetIndex();
            var toPos = to.GetIndex();
            return UnityEngine.Vector2Int.Distance(fromPos, toPos);
        }
    }
}

