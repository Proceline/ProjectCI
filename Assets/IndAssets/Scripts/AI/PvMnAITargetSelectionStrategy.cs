using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace IndAssets.Scripts.AI
{
    /// <summary>
    /// Strategy for selecting attack target
    /// </summary>
    public enum PvMnAITargetSelectionStrategy
    {
        Nearest,           // Distance nearest
        LowestHealth,      // Lowest health
        HighestHealth,     // Highest health
        LowestAttribute,   // Lowest specific attribute
        FarthestWhenAttacking // Farthest distance when attacking
    }

    /// <summary>
    /// Helper class for target selection strategies
    /// </summary>
    public static class PvMnAITargetSelectionHelper
    {
        /// <summary>
        /// Select best target cell based on strategy
        /// </summary>
        public static LevelCellBase SelectTarget(
            PvMnBattleGeneralUnit aiUnit,
            ICollection<LevelCellBase> availableTargets,
            Dictionary<LevelCellBase, List<LevelCellBase>> victimsFromCells,
            PvMnAITargetSelectionStrategy strategy,
            Func<PvMnBattleGeneralUnit, LevelCellBase, float> customAttributeEvaluator = null)
        {
            if (availableTargets == null || availableTargets.Count == 0)
                return null;

            LevelCellBase bestTarget = null;
            var bestValue = strategy == PvMnAITargetSelectionStrategy.Nearest ||
                             strategy == PvMnAITargetSelectionStrategy.LowestHealth ||
                             strategy == PvMnAITargetSelectionStrategy.LowestAttribute
                ? float.MaxValue
                : float.MinValue;

            var aiCell = aiUnit.GetCell();

            foreach (var targetCell in availableTargets)
            {
                var targetUnit = targetCell.GetUnitOnCell();
                if (!targetUnit || targetUnit.IsDead())
                    continue;

                var value = 0f;

                switch (strategy)
                {
                    case PvMnAITargetSelectionStrategy.Nearest:
                        value = GetDistance(aiCell, targetCell);
                        if (value < bestValue)
                        {
                            bestValue = value;
                            bestTarget = targetCell;
                        }
                        break;

                    case PvMnAITargetSelectionStrategy.LowestHealth:
                        value = targetUnit.RuntimeAttributes.Health.CurrentValue;
                        if (value < bestValue)
                        {
                            bestValue = value;
                            bestTarget = targetCell;
                        }
                        break;

                    case PvMnAITargetSelectionStrategy.HighestHealth:
                        value = targetUnit.RuntimeAttributes.Health.CurrentValue;
                        if (value > bestValue)
                        {
                            bestValue = value;
                            bestTarget = targetCell;
                        }
                        break;

                    case PvMnAITargetSelectionStrategy.LowestAttribute:
                        if (customAttributeEvaluator != null)
                        {
                            value = customAttributeEvaluator(aiUnit, targetCell);
                            if (value < bestValue)
                            {
                                bestValue = value;
                                bestTarget = targetCell;
                            }
                        }
                        break;

                    case PvMnAITargetSelectionStrategy.FarthestWhenAttacking:
                        // Find the farthest attack position from target
                        if (victimsFromCells.TryGetValue(targetCell, out var attackPositions) && 
                            attackPositions.Count > 0)
                        {
                            float maxDistance = 0f;
                            foreach (var attackPos in attackPositions)
                            {
                                float dist = GetDistance(attackPos, targetCell);
                                if (dist > maxDistance)
                                    maxDistance = dist;
                            }
                            value = maxDistance;
                            if (value > bestValue)
                            {
                                bestValue = value;
                                bestTarget = targetCell;
                            }
                        }
                        break;
                }
            }

            return bestTarget;
        }

        private static float GetDistance(LevelCellBase from, LevelCellBase to)
        {
            var fromPos = from.GetIndex();
            var toPos = to.GetIndex();
            return UnityEngine.Vector2Int.Distance(fromPos, toPos);
        }
    }
}

