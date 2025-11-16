using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Status
{
    public abstract class PvSoGroundStatus : ScriptableObject
    {
        private readonly HashSet<Vector2Int> _markedCellPoints = new();

        public void OnGroundPathStatusResponded(PvMnBattleGeneralUnit unit, List<LevelCellBase> path)
        {
            if (_markedCellPoints.Count <= 0) return;
            for (var i = 0; i < path.Count; i++)
            {
                var cell = path[i];
                if (_markedCellPoints.Contains(cell.GetIndex()))
                {
                    ApplyGroundStatusOnPath(unit, cell, i, path.Count);
                }
            }
        }

        public void OnGroundStatusApplied(PvMnBattleGeneralUnit unit)
        {
            if (_markedCellPoints.Count <= 0) return;
            var cell = unit.GetCell();
            if (_markedCellPoints.Contains(cell.GetIndex()))
            {
                ApplyGroundStatusOnUnit(unit);
            }
        }

        // TODO: Not in used yet
        public void OnGroundPathStatusResponded(PvMnBattleGeneralUnit unit, LevelCellBase cell)
        {
            if (_markedCellPoints.Count <= 0) return;
            if (_markedCellPoints.Contains(cell.GetIndex()))
            {
                ApplyGroundStatusOnPath(unit, cell);
            }
        }

        public void OnGroundPathStatusResponded(LevelCellBase cell)
        {
            if (_markedCellPoints.Count <= 0) return;
            if (!_markedCellPoints.Contains(cell.GetIndex())) return;
            if (cell.GetUnitOnCell() is PvMnBattleGeneralUnit battleUnit)
            {
                ApplyGroundStatusOnPath(battleUnit, cell);
            }
        }

        public void ClearAllGroundStatus()
        {
            _markedCellPoints.Clear();
        }

        public void ClearGroundStatus(LevelCellBase cell)
        {
            _markedCellPoints.Remove(cell.GetIndex());
        }

        public void AddGroundStatus(LevelCellBase cell)
        {
            _markedCellPoints.Add(cell.GetIndex());
        }

        /// <summary>
        /// Apply the status while path processing or directly applying
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="fromCell"></param>
        /// <param name="pathIndex"></param>
        /// <param name="pathLength">When Length = -1, means not in a Path</param>
        protected abstract void ApplyGroundStatusOnPath(PvMnBattleGeneralUnit unit, LevelCellBase fromCell, int pathIndex = 0,
            int pathLength = -1);

        protected abstract void ApplyGroundStatusOnUnit(PvMnBattleGeneralUnit unit);
    }
}