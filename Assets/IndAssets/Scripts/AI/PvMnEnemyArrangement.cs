using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;

namespace IndAssets.Scripts.AI
{
    public class PvMnEnemyArrangement
    {
        private readonly List<(int, PvMnBattleGeneralUnit)> _orderedEnemies = new();

        public void AddEnemy(PvMnBattleGeneralUnit enemyUnit, int chargeValue)
        {
            var enemyPair = (chargeValue, enemyUnit);
            
            var index = _orderedEnemies.BinarySearch(enemyPair, Comparer<(int, PvMnBattleGeneralUnit)>.Create(
                (a, b) => a.Item1.CompareTo(b.Item1)
            ));

            if (index < 0)
                index = ~index;

            _orderedEnemies.Insert(index, enemyPair);
        }
    }
}