using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit, List<LevelCellBase>> onPathDeterminedSupport;

        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onGlobalMovementFinishedSupport;
        
        private void OnPathDeterminedResponse(List<LevelCellBase> path)
        {
            if (!_selectedUnit) return;
            onPathDeterminedSupport.Invoke(_selectedUnit, path);
        }

        private void OnVisualMovementFinished()
        {
            if (!_selectedUnit) return;
            onGlobalMovementFinishedSupport.Invoke(_selectedUnit);
        }
    }
}