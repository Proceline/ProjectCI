using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IndAssets.Scripts.Managers
{
    public enum PvPlayerRoundState
    {
        None,
        Selected,
        Moving,
        Prepare,
        Applying
    }

    [CreateAssetMenu(fileName = "BattleStateManager", menuName = "ProjectCI Tools/SingletonBattleState")]
    public class PvSoBattleState : ScriptableObject
    {
        [SerializeField]
        private UnityEvent<PvPlayerRoundState, PvMnBattleGeneralUnit> onStateEntered;

        [SerializeField]
        private UnityEvent<PvPlayerRoundState> onStateLeft;

        private readonly Stack<PvPlayerRoundState> _states = new();

        public PvPlayerRoundState GetCurrentState 
            => _states.Count == 0 ? PvPlayerRoundState.None : _states.Peek();

        public PvPlayerRoundState PopLastState() 
            => _states.Count == 0 ? PvPlayerRoundState.None : _states.Pop();

        public bool PushState(PvPlayerRoundState state, PvMnBattleGeneralUnit unit)
        {
            var currentState = GetCurrentState;
            if (currentState == state)
            {
                return false;
            }

            if (currentState == PvPlayerRoundState.None)
            {
                Clear();
            }
            else if (!unit)
            {
                return false;
            }

            onStateLeft.Invoke(currentState);
            _states.Push(state);
            onStateEntered.Invoke(currentState, unit);

            return true;
        }

        public void Clear()
        {
            _states.Clear();
        }

        public void JumpStateToEnd()
        {
            Clear();
        }
    }
}