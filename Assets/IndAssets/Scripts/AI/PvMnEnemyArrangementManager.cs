using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Events;
using System;
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
        [SerializeField] private FeLiteGameRules gameModel;

        [SerializeField] private UnityEvent<PvMnBattleGeneralUnit> onUnitArranged;
        [SerializeField] private UnityEvent onAllThoughtsFinished;

        private readonly List<PvMnEnemyUnitThought> _orderedEnemyThoughts = new();
        private readonly Queue<PvMnEnemyUnitThought> _runtimeToRemoveThoughts = new();
        private readonly HashSet<LevelCellBase> _enemiesMovableCells = new();
        private readonly Dictionary<LevelCellBase, List<Transform>> _enemiesAttackables = new();

        [NonSerialized] private bool _isEnemyRound;

        /// <summary>
        /// Collection of all enemy thoughts, accessible by unit ID
        /// </summary>
        public IReadOnlyList<PvMnEnemyUnitThought> EnemyThoughtsCollection => _orderedEnemyThoughts;

        [Header("Preview")]

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
        /// Reset the turn, starting from the first enemy again
        /// </summary>
        public void ResetTurn()
        {
            Debug.Log("[EnemyArrangementManager] Turn reset");
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
            }
        }

        private async void ResponseOnTeamRoundEndEvent(BattleTeam endTeam)
        {
            if (endTeam == BattleTeam.Hostile)
            {
                _isEnemyRound = false;
                return;
            }
            else
            {
                _isEnemyRound = true;
            }

            await ApplyNextEnemyBehaviour();
        }

        private void ShowAggroSources(LevelCellBase targetCell)
        {
            if (!_isEnemyRound &&
                _enemiesAttackables.Count > 0 && _enemiesAttackables.TryGetValue(targetCell, out var list))
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

            if (_isEnemyRound)
            {
                return;
            }

            foreach (var enemyThought in _orderedEnemyThoughts)
            {
                var enemyUnit = enemyThought.Unit;

                if (enemyUnit.IsDead() || !enemyUnit.gameObject.activeSelf)
                {
                    // TODO: Handle Death
                    continue;
                }

                var movementPoints = enemyUnit.GetMovementPoints();
                var currentCell = enemyUnit.GetCell();

                AIRadiusInfo radiusInfo = new AIRadiusInfo(currentCell, movementPoints)
                {
                    Caster = enemyUnit,
                    bAllowBlocked = false,
                    bStopAtBlockedCell = true,
                    EffectedTeam = BattleTeam.Friendly
                };

                var usingAbility = enemyThought.SelectAbility();
                if (usingAbility.IsSupportAbility)
                {
                    continue;
                }

                var radiusField = BucketDijkstraSolutionUtils.CalculateBucket(radiusInfo, false, 10);
                foreach (var cell in radiusField.Reach)
                {
                    _enemiesMovableCells.Add(cell);
                }

                // Calculate attack field
                var attackField = BucketDijkstraSolutionUtils.ComputeAttackField(radiusField, GetCellList);
                List<LevelCellBase> GetCellList(LevelCellBase startCell)
                {
                    return usingAbility.GetShape().GetCellListPreview(enemyUnit, startCell, usingAbility.GetRadius(),
                        usingAbility.DoesAllowBlocked(), usingAbility.GetEffectedTeam());
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
                onEnemiesPreviewMoveShowed?.Invoke(_enemiesMovableCells);
                onEnemiesPreviewAggroShowed?.Invoke(_enemiesAttackables.Keys);
            }
        }

        private async Awaitable ApplyNextEnemyBehaviour()
        {
            foreach (var enemyThought in _orderedEnemyThoughts)
            {
                var enemyUnit = enemyThought.Unit;
                if (enemyUnit.IsDead() || !enemyUnit.gameObject.activeSelf)
                {
                    _runtimeToRemoveThoughts.Enqueue(enemyThought);
                    continue;
                }

                onUnitArranged?.Invoke(enemyUnit);
                await Awaitable.WaitForSecondsAsync(0.175f);

                var usingAbility = enemyThought.SelectAbility();
                var destAndTarget = enemyThought.CalculateDestinationAndTarget(usingAbility);
                var dest = destAndTarget.Item1;
                var target = destAndTarget.Item2;

                if (dest)
                {
                    gameModel.ApplyMovementToCell(enemyUnit, dest);
                    while (enemyUnit.GetCell() != dest)
                    {
                        await Awaitable.WaitForSecondsAsync(0.25f);
                    }
                }

                if (target)
                {
                    await gameModel.ApplyAbility(enemyUnit, target, usingAbility);
                    await Awaitable.WaitForSecondsAsync(0.25f);
                }

            }

            while (_runtimeToRemoveThoughts.TryDequeue(out var thought))
            {
                _orderedEnemyThoughts.Remove(thought);
            }

            onAllThoughtsFinished?.Invoke();
            gameModel.EndRound();
        }
    }
}
