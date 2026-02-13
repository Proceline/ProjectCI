using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using System.Collections.Generic;
using UnityEngine;

namespace IndAssets.Scripts.AI
{
    /// <summary>
    /// Strategy for movement behavior
    /// </summary>
    public enum PvMnAIMovementStrategy
    {
        Aggressive,     // Always move towards enemies (charge type)
        Conservative,   // Only move when enemies are in attackable range (conservative type)
        Defensive,      // Only move when enemies enter a set area range (defensive type)
        Stay
    }

    public class PvMnEnemyUnitThought : MonoBehaviour
    {
        [Header("AI Order Settings")]
        [SerializeField] private int order = 0;

        [Header("AI Strategy Settings")]
        [SerializeField] private PvMnAITargetSelectionStrategy targetSelectionStrategy = PvMnAITargetSelectionStrategy.Nearest;
        [SerializeField] private PvMnAIMovementStrategy initialStrategy = PvMnAIMovementStrategy.Aggressive;
        [SerializeField] private int defensiveRange = 5;

        [SerializeField] private PvSoVictimSelection strategy;
        //[SerializeField] private Vector2Int potentialTargetIndex;

        public PvMnAIMovementStrategy RuntimeMoveStrategy { get; private set; }

        private PvMnBattleGeneralUnit _unit;

        public int Order => order;

        public PvMnBattleGeneralUnit Unit
        {
            get
            {
                if (!_unit)
                {
                    _unit = GetComponent<PvMnBattleGeneralUnit>();
                }

                return _unit;
            }
        }

        private void Awake()
        {
            _unit = GetComponent<PvMnBattleGeneralUnit>();
            RuntimeMoveStrategy = initialStrategy;
        }

        public (LevelCellBase, LevelCellBase) CalculateDestinationAndTarget()
        {
            if (!Unit || Unit.IsDead() || RuntimeMoveStrategy == PvMnAIMovementStrategy.Stay)
            {
                return (null, null);
            }

            if (RuntimeMoveStrategy == PvMnAIMovementStrategy.Defensive)
            {
                if (!IsDefensiveTriggered())
                {
                    return (null, null);
                }

                RuntimeMoveStrategy = PvMnAIMovementStrategy.Conservative;
            }

            var ability = SelectAbility();
            return CalculateEnemyAction(ability);
        }

        /// <summary>
        /// Select ability to use. Can be overridden with custom selector
        /// </summary>
        public PvSoUnitAbility SelectAbility()
        {
            // Default: use EquippedAbility
            return _unit.AttackAbility;
        }

        private bool IsDefensiveTriggered()
        {
            var grid = TacticBattleManager.GetGrid();
            var currentCell = Unit.GetCell();
            var currentCellIndex = currentCell.GetIndex();

            if (grid)
            {
                for (var i = -defensiveRange; i <= defensiveRange; i++)
                {
                    for (var j = -defensiveRange; j <= defensiveRange; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            continue;
                        }

                        var gridIndex = currentCellIndex + new Vector2Int(i, j);
                        var cell = grid[gridIndex];
                        if (cell.GetCellTeam() == BattleTeam.Friendly)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Calculate the result where this enemy will go
        /// </summary>
        /// <param name="enemyThought"></param>
        /// <returns>First Cell is where to move, second cell is where to attack</returns>
        private (LevelCellBase, LevelCellBase) CalculateEnemyAction(PvSoUnitAbility usingAbility)
        {
            var enemyUnit = Unit;
            var movementPoints = enemyUnit.GetMovementPoints();
            var currentCell = enemyUnit.GetCell();

            AIRadiusInfo radiusInfo = new AIRadiusInfo(currentCell, movementPoints)
            {
                Caster = enemyUnit,
                bAllowBlocked = false,
                bStopAtBlockedCell = true,
                EffectedTeam = BattleTeam.Friendly
            };

            var maximumDetectRadius = 10;
            var radiusField = BucketDijkstraSolutionUtils.CalculateBucket(radiusInfo, false, maximumDetectRadius);

            // Calculate attack field
            var attackField = BucketDijkstraSolutionUtils.ComputeAttackField(radiusField, GetCellList);

            List<LevelCellBase> GetCellList(LevelCellBase startCell)
            {
                return usingAbility.GetShape().GetCellList(enemyUnit, startCell, usingAbility.GetRadius(),
                    usingAbility.DoesAllowBlocked(), usingAbility.GetEffectedTeam());
            }

            if (attackField.AllVictims.Count == 0 && RuntimeMoveStrategy == PvMnAIMovementStrategy.Conservative)
            {
                return (null, null);
            }

            if (attackField.AllVictims.Count == 0)
            {
                var futureLayers = radiusField.Layers;
                LevelCellBase potentialDest = null;
                //LevelCellBase closestCell = null;
                var currentLayer = movementPoints;

                for (int i = movementPoints + 1; i < futureLayers.Count; i++)
                {
                    currentLayer = i;
                    var futureCells = futureLayers[i];

                    //if (!closestCell)
                    //{
                    //    var lastDistance = 9999;
                    //    foreach (var futureCell in futureCells)
                    //    {
                    //        var distance = CalculateDistance(futureCell.GetIndex(), potentialTargetIndex);
                    //        if (distance < lastDistance)
                    //        {
                    //            closestCell = futureCell;
                    //        }
                    //    }
                    //}

                    foreach (var futureCell in futureCells)
                    {
                        var victimsList = GetCellList(futureCell);
                        if (victimsList.Count > 0)
                        {
                            potentialDest = futureCell;
                            break;
                        }
                    }

                    if (potentialDest)
                    {
                        break;
                    }
                }

                //if (!potentialDest)
                //{
                //    potentialDest = closestCell;
                //}

                if (potentialDest)
                {
                    var movableTarget = potentialDest;
                    for (int layer = currentLayer; layer > movementPoints; layer--)
                    {
                        if (radiusField.Parent.TryGetValue(movableTarget, out var levelCell))
                        {
                            movableTarget = levelCell;
                        }
                        else
                        {
                            movableTarget = null;
                            break;
                        }
                    }

                    if (movableTarget)
                    {
                        return (movableTarget, null);
                    }
                }
                else
                {
                    return (null, null);
                }
            }
            
            var recordedAggroPoint = -9999;
            LevelCellBase determinedVictim = null;
            foreach (var victimCell in attackField.AllVictims)
            {
                var aggroPoint = strategy.GetCellAggro(enemyUnit, victimCell, usingAbility);
                if (aggroPoint > recordedAggroPoint)
                {
                    recordedAggroPoint = aggroPoint;
                    determinedVictim = victimCell;
                }
            }

            if (determinedVictim)
            {
                if (attackField.VictimsFromCells.TryGetValue(determinedVictim, out var cellList))
                {
                    var indexOfSelf = currentCell.GetIndex();
                    var lastTotalPoint = -999;
                    LevelCellBase lastDeterminedMoveCell = null;
                    foreach (var cell in cellList)
                    {
                        var indexOfDetermined = determinedVictim.GetIndex();
                        var indexOfCell = cell.GetIndex();
                        var distanceToTarget = CalculateDistance(indexOfDetermined, indexOfCell);
                        var distanceToSelf = CalculateDistance(indexOfCell, indexOfSelf);
                        var distanceTotalPoint = 2 * distanceToTarget - distanceToSelf;
                        if (distanceTotalPoint > lastTotalPoint)
                        {
                            lastTotalPoint = distanceTotalPoint;
                            lastDeterminedMoveCell = cell;
                        }
                    }

                    if (lastDeterminedMoveCell)
                    { 
                        if (RuntimeMoveStrategy == PvMnAIMovementStrategy.Conservative)
                        {
                            RuntimeMoveStrategy = PvMnAIMovementStrategy.Aggressive;
                        }

                        return (lastDeterminedMoveCell, determinedVictim);
                    }
                }
            }

            return (null, null);
        }

        private int CalculateDistance(Vector2Int index0, Vector2Int index1)
        {
            return Mathf.Abs(index0.x - index1.x) + Mathf.Abs(index0.y - index1.y);
        }
    }
}