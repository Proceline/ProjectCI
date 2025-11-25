using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using UnityEngine;

namespace IndAssets.Scripts.AI
{
    [CreateAssetMenu(fileName = "AI Arrangement", menuName = "ProjectCI Tools/AI/Create Arrangement")]
    public class PvSoEnemyArrangement : ScriptableObject
    {
        private readonly List<(int chargeValue, PvMnBattleGeneralUnit unit)> _orderedEnemies = new();
        private readonly Dictionary<string, PvMnEnemyUnitThought> _enemyThoughtsHash = new();
        [NonSerialized] private int _currentIndex = 0;
        
        public IDictionary<string, PvMnEnemyUnitThought> EnemyThoughtsCollection => _enemyThoughtsHash;

        public void AddEnemy(PvMnBattleGeneralUnit enemyUnit, int chargeValue)
        {
            var thought = enemyUnit.GetComponent<PvMnEnemyUnitThought>();
            if (!thought) return;
            
            var enemyPair = (chargeValue, enemyUnit);
            
            var index = _orderedEnemies.BinarySearch(enemyPair, Comparer<(int, PvMnBattleGeneralUnit)>.Create(
                (a, b) => a.Item1.CompareTo(b.Item1)
            ));

            if (index < 0)
                index = ~index;

            _orderedEnemies.Insert(index, enemyPair);
            _enemyThoughtsHash.Add(enemyUnit.ID, thought);
        }

        public bool TryGetNextEnemy(out PvMnBattleGeneralUnit outUnit)
        {
            if (_currentIndex >= _orderedEnemies.Count)
            {
                outUnit = null;
                return false;
            }

            // Skip dead or units with no action points
            while (_currentIndex < _orderedEnemies.Count)
            {
                var enemy = _orderedEnemies[_currentIndex].unit;
                if (!enemy.IsDead() && 
                    (enemy.GetCurrentMovementPoints() > 0 || enemy.GetCurrentActionPoints() > 0))
                {
                    outUnit = enemy;
                    return true;
                }
                _currentIndex++;
            }

            outUnit = null;
            return false;
        }

        public void MarkCurrentEnemyFinished()
        {
            _currentIndex++;
        }

        public void ResetTurn()
        {
            _currentIndex = 0;
        }

        public bool HasEnemiesRemaining()
        {
            return _currentIndex < _orderedEnemies.Count;
        }

        public int GetRemainingEnemyCount()
        {
            return _orderedEnemies.Count - _currentIndex;
        }
    }
}