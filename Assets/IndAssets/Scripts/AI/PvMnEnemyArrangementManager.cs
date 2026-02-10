using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.TacticTool.Formula.Concrete;
using ProjectCI.Utilities.Runtime.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IndAssets.Scripts.AI
{
    /// <summary>
    /// MonoBehaviour version of enemy arrangement manager.
    /// Automatically finds and manages all enemy units in the scene based on their order values.
    /// </summary>
    public class PvMnEnemyArrangementManager : MonoBehaviour
    {
        [SerializeField] private PvSoBattleTeamEvent onTeamRoundEndEvent;
        [SerializeField] private PvSoSimpleVoidEvent onBattleStartedEvent;
        
        private readonly List<PvMnEnemyUnitThought> _orderedEnemyThoughts = new();
        private readonly HashSet<LevelCellBase> _enemiesMovableCells = new();
        private readonly Dictionary<LevelCellBase, List<Transform>> _enemiesAttackables = new();

        private int _currentIndex = 0;

        /// <summary>
        /// Collection of all enemy thoughts, accessible by unit ID
        /// </summary>
        public IReadOnlyList<PvMnEnemyUnitThought> EnemyThoughtsCollection => _orderedEnemyThoughts;

        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onEnemyPrepared;
        [SerializeField] private UnityEvent<LevelCellBase> onEnemyMoving;
        [SerializeField] private UnityEvent<LevelCellBase, PvSoUnitAbility> onEnemyActing;

        [Header("Preview")]

        [SerializeField] private FormulaCollection formulaCollection;

        [SerializeField] private UnityEvent<ICollection<LevelCellBase>> onEnemiesPreviewAggroShowed;
        [SerializeField] private UnityEvent<ICollection<LevelCellBase>> onEnemiesPreviewMoveShowed;
        [SerializeField] private UnityEvent onEnemiesPreviewHidden;

        [SerializeField] private PvMnAggroLine aggroLinePrefab;
        private readonly List<PvMnAggroLine> _linesPool = new List<PvMnAggroLine>();

        [SerializeField] private PvSoLevelCellEvent onAggroCellHoveredEvent;
        [SerializeField] private PvSoLevelCellEvent onAggroCellUnhoveredEvent;

        private void Start()
        {
            onTeamRoundEndEvent.RegisterCallback(ResponseOnTeamRoundEndEvent);
            onBattleStartedEvent.RegisterCallback(InitializeEnemies);
            onAggroCellHoveredEvent.RegisterCallback(ShowAggroSources);
            onAggroCellUnhoveredEvent.RegisterCallback(HideAggroSources);
        }

        private void OnDestroy()
        {
            onTeamRoundEndEvent.UnregisterCallback(ResponseOnTeamRoundEndEvent);
            onBattleStartedEvent.UnregisterCallback(InitializeEnemies);
            onAggroCellHoveredEvent.UnregisterCallback(ShowAggroSources);
            onAggroCellUnhoveredEvent.UnregisterCallback(HideAggroSources);
        }

        /// <summary>
        /// Automatically find all PvMnEnemyUnitThought components in the scene and sort them by order
        /// </summary>
        public void InitializeEnemies()
        {
            _orderedEnemyThoughts.Clear();
            _currentIndex = 0;

            // Find all enemy thought components in the scene
            var allEnemyThoughts = FindObjectsByType<PvMnEnemyUnitThought>(FindObjectsSortMode.None);

            // Filter out thoughts that don't have a valid unit or are already dead
            foreach (var thought in allEnemyThoughts)
            {
                var unit = thought.GetComponent<PvMnBattleGeneralUnit>();
                if (unit != null && !unit.IsDead())
                {
                    _orderedEnemyThoughts.Add(thought);
                }
            }

            // Sort by order value (ascending - lower order acts first)
            _orderedEnemyThoughts.Sort((a, b) => a.Order.CompareTo(b.Order));

            Debug.Log($"[EnemyArrangementManager] Initialized {_orderedEnemyThoughts.Count} enemy units");
        }

        /// <summary>
        /// Add a single enemy thought to the arrangement
        /// </summary>
        public void AddEnemyThought(PvMnEnemyUnitThought enemyThought)
        {
            if (enemyThought == null)
            {
                Debug.LogWarning("[EnemyArrangementManager] Attempted to add null enemy thought");
                return;
            }

            var unit = enemyThought.GetComponent<PvMnBattleGeneralUnit>();
            if (unit == null)
            {
                Debug.LogWarning("[EnemyArrangementManager] Enemy thought has no PvMnBattleGeneralUnit component");
                return;
            }

            // Insert in sorted order
            int insertIndex = _orderedEnemyThoughts.Count;
            for (int i = 0; i < _orderedEnemyThoughts.Count; i++)
            {
                if (enemyThought.Order < _orderedEnemyThoughts[i].Order)
                {
                    insertIndex = i;
                    break;
                }
            }

            _orderedEnemyThoughts.Insert(insertIndex, enemyThought);
            Debug.Log($"[EnemyArrangementManager] Added enemy unit with order {enemyThought.Order}");
        }

        /// <summary>
        /// Try to get the next enemy unit that should act
        /// </summary>
        /// <param name="outUnit">The next enemy unit to act</param>
        /// <returns>True if there is a next enemy, false otherwise</returns>
        public bool TryGetNextEnemy(out PvMnEnemyUnitThought outThought)
        {
            if (_currentIndex >= _orderedEnemyThoughts.Count)
            {
                outThought = null;
                return false;
            }

            // Skip dead units or units with no action points
            while (_currentIndex < _orderedEnemyThoughts.Count)
            {
                var enemyThought = _orderedEnemyThoughts[_currentIndex];
                var enemyUnit = enemyThought.Unit;
                if (enemyUnit && !enemyUnit.IsDead() &&
                    (enemyUnit.GetCurrentMovementPoints() > 0 || enemyUnit.GetCurrentActionPoints() > 0))
                {
                    outThought = enemyThought;
                    return true;
                }

                _currentIndex++;
            }

            outThought = null;
            return false;
        }

        /// <summary>
        /// Get the current enemy thought component
        /// </summary>
        /// <param name="outThought">The current enemy thought</param>
        /// <returns>True if there is a current enemy thought, false otherwise</returns>
        public bool TryGetCurrentEnemyThought(out PvMnEnemyUnitThought outThought)
        {
            if (_currentIndex >= 0 && _currentIndex < _orderedEnemyThoughts.Count)
            {
                outThought = _orderedEnemyThoughts[_currentIndex];
                return true;
            }

            outThought = null;
            return false;
        }

        /// <summary>
        /// Mark the current enemy as finished and move to the next
        /// </summary>
        public void MarkCurrentEnemyFinished()
        {
            _currentIndex++;
        }

        /// <summary>
        /// Reset the turn, starting from the first enemy again
        /// </summary>
        public void ResetTurn()
        {
            _currentIndex = 0;
            Debug.Log("[EnemyArrangementManager] Turn reset");
        }

        /// <summary>
        /// Check if there are any enemies remaining to act
        /// </summary>
        public bool HasEnemiesRemaining()
        {
            return _currentIndex < _orderedEnemyThoughts.Count;
        }

        /// <summary>
        /// Get the number of enemies remaining to act
        /// </summary>
        public int GetRemainingEnemyCount()
        {
            return Mathf.Max(0, _orderedEnemyThoughts.Count - _currentIndex);
        }

        /// <summary>
        /// Get the total number of enemies in the arrangement
        /// </summary>
        public int GetTotalEnemyCount()
        {
            return _orderedEnemyThoughts.Count;
        }

        /// <summary>
        /// Remove dead enemies from the list
        /// </summary>
        public void CleanupDeadEnemies()
        {
            int removedCount = _orderedEnemyThoughts.RemoveAll(thought =>
            {
                var unit = thought.GetComponent<PvMnBattleGeneralUnit>();
                return unit == null || unit.IsDead();
            });

            if (removedCount > 0)
            {
                Debug.Log($"[EnemyArrangementManager] Removed {removedCount} dead enemies");
                // Adjust current index if needed
                _currentIndex = Mathf.Min(_currentIndex, _orderedEnemyThoughts.Count);
            }
        }

        private async void ResponseOnTeamRoundEndEvent(BattleTeam endTeam)
        {
            if (endTeam == BattleTeam.Hostile)
            {
                return;
            }
            _currentIndex = 0;
            await ApplyNextEnemyBehaviour();
        }

        private void ShowAggroSources(LevelCellBase targetCell)
        {
            if (_enemiesAttackables.Count > 0 && _enemiesAttackables.TryGetValue(targetCell, out var list))
            {
                foreach (var line in _linesPool)
                {
                    line.Hide();
                }

                for (int i = 0; i < list.Count; i++)
                {
                    if (i >= _linesPool.Count)
                    {
                        _linesPool.Add(Instantiate(aggroLinePrefab));
                    }

                    Vector3 startPos = list[i].position + Vector3.up * 1.5f;
                    Vector3 endPos = targetCell.transform.position;

                    _linesPool[i].DrawCurve(startPos, endPos);
                }
            }
        }

        private void HideAggroSources(LevelCellBase targetCell)
        {
            foreach (var line in _linesPool)
            {
                line.Hide();
            }
        }

        /// <summary>
        /// Binded to Controller's preview input action Event.
        /// Calculate and show all enemies' potential move and attack cells based on their current positions and abilities.
        /// </summary>
        /// <param name="showingView"></param>
        public void PreviewAllEnemiesActions(bool showingView)
        {
            _enemiesMovableCells.Clear();
            _enemiesAttackables.Clear();

            foreach (var enemyThought in _orderedEnemyThoughts)
            {
                var enemyUnit = enemyThought.Unit;

                var movementPoints = enemyUnit.RuntimeAttributes.GetAttributeValue(formulaCollection.MovementAttributeType);
                var currentCell = enemyUnit.GetCell();

                AIRadiusInfo radiusInfo = new AIRadiusInfo(currentCell, movementPoints)
                {
                    Caster = enemyUnit,
                    bAllowBlocked = false,
                    bStopAtBlockedCell = true,
                    EffectedTeam = BattleTeam.Friendly
                };

                var radiusField = BucketDijkstraSolutionUtils.CalculateBucket(radiusInfo, false, 10);
                //var availableMoveCells = radiusField.Dist.Keys;
                foreach (var cellPair in radiusField.Dist)
                {
                    var cell = cellPair.Key;
                    _enemiesMovableCells.Add(cell);
                }

                var attackAbility = enemyThought.Unit.AttackAbility;

                // Calculate attack field
                var attackField = BucketDijkstraSolutionUtils.ComputeAttackField(radiusField, GetCellList);
                List<LevelCellBase> GetCellList(LevelCellBase startCell)
                {
                    return attackAbility.GetShape().GetCellListPreview(enemyUnit, startCell, attackAbility.GetRadius(),
                        attackAbility.DoesAllowBlocked(), attackAbility.GetEffectedTeam());
                }

                foreach (var victimCell in attackField.AllVictims)
                {
                    if (!_enemiesAttackables.TryGetValue(victimCell, out var list))
                    {
                        list = new List<Transform>();
                        _enemiesAttackables[victimCell] = list;
                    }

                    if (!list.Contains(enemyThought.transform))
                    {
                        list.Add(enemyThought.transform);
                    }
                }
            }

            if (showingView)
            {
                onEnemiesPreviewAggroShowed?.Invoke(_enemiesAttackables.Keys);
                onEnemiesPreviewMoveShowed?.Invoke(_enemiesMovableCells);
            }
        }

        private async Awaitable ApplyNextEnemyBehaviour()
        {
            while (TryGetNextEnemy(out var nextEnemyThought))
            {
                var nextEnemy = nextEnemyThought.Unit;
                onEnemyPrepared.Invoke(nextEnemy);

                var result = nextEnemyThought.CalculateBestAction();

                if (!result.HasAction)
                {
                    Debug.LogError("No action required for this enemy");
                    return;
                }

                await Awaitable.WaitForSecondsAsync(0.5f);

                var targetCell = result.MoveToCell;
                var targetVictim = result.AttackTargetCell;

                onEnemyMoving.Invoke(targetCell);

                while (nextEnemy.GetCell() != targetCell)
                {
                    await Awaitable.WaitForSecondsAsync(0.25f);
                }

                onEnemyActing.Invoke(targetVictim, result.AbilityToUse);

                while (nextEnemy.GetCurrentState() == UnitBattleState.AbilityConfirming)
                {
                    await Awaitable.WaitForSecondsAsync(0.25f);
                }
            }
        }


#if UNITY_EDITOR
        [ContextMenu("Debug: Show Enemy Order")]
        private void DebugShowEnemyOrder()
        {
            Debug.Log("=== Enemy Order ===");
            for (int i = 0; i < _orderedEnemyThoughts.Count; i++)
            {
                var thought = _orderedEnemyThoughts[i];
                var unit = thought.GetComponent<PvMnBattleGeneralUnit>();
                string status = i == _currentIndex ? " <- CURRENT" : "";
                Debug.Log($"{i}: Order={thought.Order}, Unit={unit?.name ?? "NULL"}, Dead={unit?.IsDead() ?? true}{status}");
            }
            Debug.Log($"Current Index: {_currentIndex}/{_orderedEnemyThoughts.Count}");
        }

        [ContextMenu("Debug: Re-Initialize Enemies")]
        private void DebugReInitialize()
        {
            InitializeEnemies();
            DebugShowEnemyOrder();
        }
#endif
    }
}
