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
        
        [SerializeField] 
        private UnityEvent<PvMnBattleGeneralUnit> onRoundEndedForEachUnit;
        
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
                var statusContainer = unit.GetStatusEffectContainer();
                var statusDataCollection = statusContainer.GetStatusList();
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

                while (toRemoveIndexList.TryPop(out var index))
                {
                    statusContainer.RemoveStatusByIndex(index);
                }
            }
        }

        public void OnTeamRoundEndResponse(BattleTeam team)
        {
            var allUnitsInBattle = _unitIdToBattleUnitHash.Values;
            foreach (var unit in allUnitsInBattle)
            {
                if (unit.IsDead()) continue;
                if (unit.GetTeam() == team)
                {
                    ArchiveUnitBehaviourPoints(unit, true, true);
                }

                onRoundEndedForEachUnit.Invoke(unit);
                onUnitStatusUpdated.Invoke(unit);
            }

            CurrentTeam = team == BattleTeam.Friendly? BattleTeam.Hostile : BattleTeam.Friendly;
            BeginTeamTurn(CurrentTeam);
        }
    }
}