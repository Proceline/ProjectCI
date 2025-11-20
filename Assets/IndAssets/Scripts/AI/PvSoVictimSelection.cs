using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace IndAssets.Scripts.AI
{
    // [CreateAssetMenu(fileName = "PvSoVictimSelection", menuName = "ProjectCI Tools/AI/Create VictimSelection")]
    public abstract class PvSoVictimSelection : ScriptableObject
    {
        private readonly SortedList<int, LevelCellBase> _sortedTargets = new();
        
        protected abstract int GetCellAggro(PvMnBattleGeneralUnit aiOwner, LevelCellBase cell);

        public LevelCellBase GetTargetCell(PvMnBattleGeneralUnit aiOwner, ICollection<LevelCellBase> allVictims,
            Dictionary<LevelCellBase, List<LevelCellBase>> victimsFromCells)
        {
            foreach (var victim in allVictims)
            {
                var aggroResult = GetCellAggro(aiOwner, victim);
                _sortedTargets.Add(aggroResult, victim);
            }

            var lastIndex = _sortedTargets.Count - 1;
            var lastKey = _sortedTargets.Keys[lastIndex];
            var lastTarget = _sortedTargets[lastKey];
            return lastTarget;
        }
    }
}
