using System.Collections.Generic;
using IndAssets.Scripts.Passives.Status;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public partial class FeLiteGameRules
    {
        [Header("Movement Utils")]
        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit, List<LevelCellBase>> onPathDeterminedSupport;

        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onGlobalMovementFinishedSupport;
        
        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit, IBattleStatus> onUnitStatusCalculatedGlobally;

        [SerializeField]
        private UnityEvent<PvMnBattleGeneralUnit> onUnitStatusUpdated;
        
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

        public void CalculateAllUnitsStatusOnRoundEnded(BattleTeam team)
        {
            foreach (var unitPair in _unitIdToBattleUnitHash)
            {
                var unit = unitPair.Value;
                var statusList = unit.GetStatusEffectContainer();
                var statusDataCollection = statusList.GetStatusList();
                var toRemoveIndexList = new Stack<int>();
                for (var i = 0; i < statusDataCollection.Count; i++)
                {
                    var statusData = statusDataCollection[i];
                    if (statusData.StatusTag == nameof(PvSoPassiveStatusFire))
                    {
                        onUnitStatusCalculatedGlobally.Invoke(unit, statusData);
                    }

                    if (statusData.IsBeingDisposed())
                    {
                        toRemoveIndexList.Push(i);
                    }
                }

                var isUpdateRequired = false;
                while (toRemoveIndexList.TryPop(out var index))
                {
                    statusDataCollection.RemoveAt(index);
                    isUpdateRequired = true;
                }

                if (isUpdateRequired)
                {
                    onUnitStatusUpdated.Invoke(unit);
                }
            }
        }
    }
}