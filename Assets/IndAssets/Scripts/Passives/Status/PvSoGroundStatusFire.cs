using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Status
{
    [CreateAssetMenu(fileName = "New Fire Ground Status", menuName = "ProjectCI GroundStatus/FireWall", order = 1)]
    public class PvSoGroundStatusFire : PvSoGroundStatus
    {
        [SerializeField] private PvSoPassiveStatusFire relatedDeBuff;
        [SerializeField] private PvSoStatusApplyEvent raiserStatusApplyEvent;

        private readonly Dictionary<IEventOwner , Action> _pendingActionsCollection = new(); 
        
        protected override void ApplyGroundStatusOnPath(PvMnBattleGeneralUnit unit, LevelCellBase fromCell, int pathIndex = 0,
            int pathLength = -1)
        {
            var targetUnit = unit;
            if (!_pendingActionsCollection.TryGetValue(targetUnit, out var existedAction))
            {
                existedAction = () => ApplyGroundStatusOnUnit(targetUnit);
            }
            else
            {
                existedAction += () => ApplyGroundStatusOnUnit(targetUnit);
            }
            _pendingActionsCollection[targetUnit] = existedAction;
        }

        protected override void ApplyGroundStatusOnUnit(PvMnBattleGeneralUnit unit)
        {
            relatedDeBuff.AccumulateStatus(unit, 1);
            raiserStatusApplyEvent.Raise(unit, relatedDeBuff);
        }

        public void DetermineProcessOnStateChanged(IEventOwner owner, UnitStateEventParam stateEventParam)
        {
            if (_pendingActionsCollection.Count == 0) return;
            
            var state = stateEventParam.battleState;
            var stateBehaviour = stateEventParam.behaviour;

            switch (state)
            {
                case UnitBattleState.UsingAbility when stateBehaviour == UnitStateBehaviour.Popping:
                    _pendingActionsCollection.Remove(owner);
                    break;
                case UnitBattleState.Finished when stateBehaviour == UnitStateBehaviour.Clear:
                {
                    if (_pendingActionsCollection.TryGetValue(owner, out var pendingAction))
                    {
                        pendingAction.Invoke();
                        _pendingActionsCollection.Remove(owner);
                    }

                    break;
                }
            }
        }
    }
}