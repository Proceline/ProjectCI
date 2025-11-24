using System;
using System.Collections.Generic;
using System.Linq;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace IndAssets.Scripts.AI
{
    public class PvMnEnemyUnitThought : MonoBehaviour
    {
        [Header("AI Strategy Settings")]
        [SerializeField] private PvMnAITargetSelectionStrategy targetSelectionStrategy = PvMnAITargetSelectionStrategy.Nearest;
        [SerializeField] private PvMnAIMovementStrategy movementStrategy = PvMnAIMovementStrategy.Aggressive;
        [SerializeField] private int defensiveRange = 5;

        [Header("Ability Selection")]
        [SerializeField] private bool useCustomAbilitySelector = false;
        
        // Custom evaluators for extensibility
        public Func<PvMnBattleGeneralUnit, LevelCellBase, float> CustomAttributeEvaluator { get; set; }
        public Func<PvMnBattleGeneralUnit, PvSoUnitAbility, LevelCellBase, bool> CustomAbilitySelector { get; set; }

        private PvMnBattleGeneralUnit _unit;

        private void Awake()
        {
            _unit = GetComponent<PvMnBattleGeneralUnit>();
        }

        /// <summary>
        /// Calculate the best action for this AI unit
        /// </summary>
        public PvMnAIDecisionResult CalculateBestAction()
        {
            if (!_unit || _unit.IsDead())
            {
                return new PvMnAIDecisionResult { ShouldTakeRest = true };
            }

            // Check if unit has any action points
            if (_unit.GetCurrentMovementPoints() <= 0 && _unit.GetCurrentActionPoints() <= 0)
            {
                return new PvMnAIDecisionResult { ShouldTakeRest = true };
            }

            var result = new PvMnAIDecisionResult();

            // Calculate movement and attack fields
            var movementPoints = _unit.GetCurrentMovementPoints();
            var currentCell = _unit.GetCell();

            AIRadiusInfo radiusInfo = new AIRadiusInfo(currentCell, movementPoints)
            {
                Caster = _unit,
                bAllowBlocked = false,
                bStopAtBlockedCell = true,
                EffectedTeam = BattleTeam.Friendly
            };

            var radiusField = BucketDijkstraSolutionUtils.CalculateBucket(radiusInfo, false, 10);
            var availableMoveCells = radiusField.Dist.Keys.ToList();

            // Get ability to use (default or custom)
            var ability = SelectAbility();
            if (ability == null)
            {
                return new PvMnAIDecisionResult { ShouldTakeRest = true };
            }

            // Calculate attack field
            Dictionary<LevelCellBase, List<LevelCellBase>> victimsFromCells = null;
            ICollection<LevelCellBase> allVictims = null;

            if (ability != null)
            {
                var attackField = BucketDijkstraSolutionUtils.ComputeAttackField(radiusField, GetCellList);

                List<LevelCellBase> GetCellList(LevelCellBase startCell)
                {
                    return ability.GetShape().GetCellList(_unit, startCell, ability.GetRadius(),
                        ability.DoesAllowBlocked(), ability.GetEffectedTeam());
                }

                victimsFromCells = attackField.VictimsFromCells;
                allVictims = attackField.AllVictims;
            }

            // Select target based on strategy
            LevelCellBase targetCell = null;
            if (allVictims != null && allVictims.Count > 0)
            {
                targetCell = PvMnAITargetSelectionHelper.SelectTarget(
                    _unit,
                    allVictims,
                    victimsFromCells,
                    targetSelectionStrategy,
                    CustomAttributeEvaluator
                );
            }

            // Determine if should move
            bool shouldMove = PvMnAIMovementHelper.ShouldMove(
                _unit,
                movementStrategy,
                availableMoveCells,
                allVictims,
                victimsFromCells,
                defensiveRange
            );

            // Find best movement cell
            LevelCellBase moveToCell = null;
            if (shouldMove && availableMoveCells.Count > 0)
            {
                moveToCell = PvMnAIMovementHelper.FindBestMovementCell(
                    _unit,
                    movementStrategy,
                    availableMoveCells,
                    targetCell,
                    victimsFromCells
                );
            }

            // Determine attack target
            LevelCellBase attackTarget = null;
            if (targetCell != null)
            {
                // Check if we can attack from current position
                var finalPosition = moveToCell ?? currentCell;
                if (victimsFromCells != null && 
                    victimsFromCells.TryGetValue(targetCell, out var attackPositions) &&
                    attackPositions.Contains(finalPosition))
                {
                    attackTarget = targetCell;
                }
                else if (moveToCell != null && 
                         victimsFromCells != null &&
                         victimsFromCells.TryGetValue(targetCell, out var newAttackPositions) &&
                         newAttackPositions.Contains(moveToCell))
                {
                    attackTarget = targetCell;
                }
            }

            result.MoveToCell = moveToCell;
            result.AttackTargetCell = attackTarget;
            result.AbilityToUse = ability;
            result.ShouldTakeRest = !result.HasAction;

            return result;
        }

        /// <summary>
        /// Select ability to use. Can be overridden with custom selector
        /// </summary>
        private PvSoUnitAbility SelectAbility()
        {
            if (useCustomAbilitySelector && CustomAbilitySelector != null)
            {
                var abilities = _unit.GetAttackAbilities();
                foreach (var ability in abilities)
                {
                    // Custom selector would need target cell, but we don't have it yet
                    // For now, return first available ability if custom selector is set
                    if (ability != null)
                        return ability;
                }
            }

            // Default: use EquippedAbility
            return _unit.EquippedAbility;
        }

        // Legacy method kept for compatibility
        public void ShowTargetPawnField(PvMnBattleGeneralUnit inSceneUnit, bool allowBlock, bool toTeammate)
        {
            var startUnit = inSceneUnit;
            if (!startUnit)
            {
                Debug.LogError("You must drag and drop a unit!");
                return;
            }

            var movementPoint = startUnit.GetCurrentMovementPoints();
            
            AIRadiusInfo radiusInfo = new AIRadiusInfo(startUnit.GetCell(), movementPoint)
            {
                Caster = startUnit,
                bAllowBlocked = allowBlock,
                bStopAtBlockedCell = true,
                EffectedTeam = BattleTeam.Friendly
            };

            var radiusField = BucketDijkstraSolutionUtils.CalculateBucket(radiusInfo, false, 10);

            foreach (var pair in radiusField.Dist)
            {
                var cell = pair.Key;
                var value = pair.Value;
            }

            var ability = startUnit.EquippedAbility;
            if (ability)
            {
                var attackField = BucketDijkstraSolutionUtils.ComputeAttackField(radiusField, GetCellList);

                List<LevelCellBase> GetCellList(LevelCellBase startCell)
                {
                    return ability.GetShape().GetCellList(startUnit, startCell, ability.GetRadius(),
                        ability.DoesAllowBlocked(), ability.GetEffectedTeam());
                }

                var victimDic = attackField.VictimsFromCells;
            }
        }
    }
}