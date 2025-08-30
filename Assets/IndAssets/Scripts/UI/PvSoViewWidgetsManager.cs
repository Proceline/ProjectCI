using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.Runtime.GUI.Battle
{
    [CreateAssetMenu(fileName = "New ViewWidgetsManager", menuName = "ProjectCI UI/Create ViewWidgetsManager", order = 1)]
    public class PvSoViewWidgetsManager : ScriptableObject
    {
        private readonly Stack<GameObject> _battleActionsPanelStack = new();
        
        public void SwitchEnabledConfirmAction(PvMnBattleGeneralUnit unit, UnitBattleState state)
        {
            switch (state)
            {
                case UnitBattleState.UsingAbility:
                    // if widget already disable, enable it and don't need push because it must from CANCEL
                    if (_battleActionsPanelStack.TryPeek(out var widgetsCollection))
                    {
                        widgetsCollection.SetActive(false);
                    }
                    _battleActionsPanelStack.Push(new GameObject());
                    break;
                case UnitBattleState.AbilityTargeting:
                    break;
                case UnitBattleState.Moving:
                    break;
                case UnitBattleState.Finished:
                    break;
                case UnitBattleState.MovingProgress:
                case UnitBattleState.Idle:
                case UnitBattleState.AbilityConfirming:
                default:
                    // Empty
                    break;
            }
        }

    }
}