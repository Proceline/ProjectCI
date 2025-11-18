using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities.Projectiles;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.Utilities.Runtime.Pools;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Status
{
    public abstract class PvSoGroundStatus : ScriptableObject
    {
        private readonly Dictionary<Vector2Int, int> _markedCellPoints = new();
        [SerializeField] protected int initDuration = 2;
        public PvMnVisualEffect groundEffectPrefab;

        private readonly Dictionary<Vector2Int, GameObject> _recordedVisualObjects = new();

        public void OnGroundPathStatusResponded(PvMnBattleGeneralUnit unit, List<LevelCellBase> path)
        {
            if (_markedCellPoints.Count <= 0) return;
            for (var i = 0; i < path.Count; i++)
            {
                var cell = path[i];
                if (_markedCellPoints.ContainsKey(cell.GetIndex()))
                {
                    ApplyGroundStatusOnPath(unit, cell, i, path.Count);
                }
            }
        }

        public void OnGroundStatusApplied(PvMnBattleGeneralUnit unit)
        {
            if (_markedCellPoints.Count <= 0) return;
            var cell = unit.GetCell();
            if (_markedCellPoints.ContainsKey(cell.GetIndex()))
            {
                ApplyGroundStatusOnUnit(unit);
            }
        }

        // TODO: Not in used yet
        public void OnGroundPathStatusResponded(PvMnBattleGeneralUnit unit, LevelCellBase cell)
        {
            if (_markedCellPoints.Count <= 0) return;
            if (_markedCellPoints.ContainsKey(cell.GetIndex()))
            {
                ApplyGroundStatusOnPath(unit, cell);
            }
        }

        public void OnGroundPathStatusResponded(LevelCellBase cell)
        {
            if (_markedCellPoints.Count <= 0) return;
            if (!_markedCellPoints.ContainsKey(cell.GetIndex())) return;
            if (cell.GetUnitOnCell() is PvMnBattleGeneralUnit battleUnit)
            {
                ApplyGroundStatusOnPath(battleUnit, cell);
            }
        }

        public void ReduceDuration()
        {
            var toRemoveList = new List<Vector2Int>();
            var toModifyList = new List<Vector2Int>();
            foreach (var pair in _markedCellPoints)
            {
                if (pair.Value - 1 <= 0)
                {
                    toRemoveList.Add(pair.Key);
                }
                else
                {
                    toModifyList.Add(pair.Key);
                }
            }

            foreach (var key in toRemoveList)
            {
                _markedCellPoints.Remove(key);
            }
            
            foreach (var key in toModifyList)
            {
                var oldValue = _markedCellPoints[key];
                _markedCellPoints[key] = oldValue - 1;
            }
        }

        public void AddGroundStatus(LevelCellBase cell)
        {
            _markedCellPoints.Add(cell.GetIndex(), initDuration);
        }

        /// <summary>
        /// Apply the status while path processing or directly applying
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="fromCell"></param>
        /// <param name="pathIndex"></param>
        /// <param name="pathLength">When Length = -1, means not in a Path</param>
        protected abstract void ApplyGroundStatusOnPath(PvMnBattleGeneralUnit unit, LevelCellBase fromCell,
            int pathIndex = 0,
            int pathLength = -1);

        protected abstract void ApplyGroundStatusOnUnit(PvMnBattleGeneralUnit unit);

        #region Visual

        public void RefreshVisualGroundStatus(List<LevelCellBase> effectedCells)
        {
            foreach (var cell in effectedCells)
            {
                RefreshVisualGroundStatus(cell);
            }
        }

        public void RefreshVisualGroundStatus(LevelCellBase cell)
        {
            if (_recordedVisualObjects.ContainsKey(cell.GetIndex())) return;
            var newEffect = MnObjectPool.Instance.Get(groundEffectPrefab.gameObject);
            newEffect.transform.position = cell.transform.position;
            _recordedVisualObjects.Add(cell.GetIndex(), newEffect);
        }

        public void RefreshVisualGroundStatus()
        {
            var toRemoveList = new List<Vector2Int>();
            foreach (var pair in _recordedVisualObjects)
            {
                if (!_markedCellPoints.ContainsKey(pair.Key))
                {
                    toRemoveList.Add(pair.Key);
                }
            }
            
            foreach (var key in toRemoveList)
            {
                if (_recordedVisualObjects.Remove(key, out var effect))
                {
                    MnObjectPool.Instance.Return(effect);
                }
            }
        }

        #endregion
    }
}