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

            // Calculate movement and attack fields
            var movementPoints = _unit.GetCurrentMovementPoints();
            var currentCell = _unit.GetCell();

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

            var radiusField = BucketDijkstraSolutionUtils.CalculateBucket(radiusInfo, false, 10);

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
                Debug.LogError("Dict: " + radiusField.Dist.Count + ", Reach: " + radiusField.Reach.Count);
                var cellLayers = radiusField.Layers;
                var count = 0;
                Debug.LogError("How many layers:" + cellLayers.Count);
                for (int i = 0; i < cellLayers.Count; i++)
                {
                    count += cellLayers[i].Count;
                }
                Debug.LogError(count + " " + radiusInfo.Radius);

                return (null, null);
                // TODO
                //        public readonly Dictionary<LevelCellBase, int> Dist = new();
                //public readonly HashSet<LevelCellBase> Reach = new();
                //public readonly List<List<LevelCellBase>> Layers = new();
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