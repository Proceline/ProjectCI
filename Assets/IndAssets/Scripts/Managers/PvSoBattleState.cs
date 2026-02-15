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

            if (state == PvPlayerRoundState.None)
            {
                Clear();
            }
            else if (!unit)
            {
                return false;
            }

            _states.Push(state);
            onStateEntered.Invoke(state, unit);

            return true;
        }

        public PvPlayerRoundState CancelState(PvMnBattleGeneralUnit unit)
        {
            var lastState = PopLastState();
            if (lastState != PvPlayerRoundState.None)
            {
                onStateLeft.Invoke(lastState);
                var currentState = GetCurrentState;
                onStateEntered.Invoke(currentState, unit);
            }
            return lastState;
        }

        public void RegisterCallbackOnEnter(UnityAction<PvPlayerRoundState, PvMnBattleGeneralUnit> callback)
        {
            onStateEntered.AddListener(callback);
        }

        public void UnregisterCallbackOnEnter(UnityAction<PvPlayerRoundState, PvMnBattleGeneralUnit> callback)
        {
            onStateEntered.RemoveListener(callback);
        }

        public void Clear()
        {
            onStateEntered.Invoke(PvPlayerRoundState.None, null);
            _states.Clear();
        }

        public void JumpStateToEnd()
        {
            Clear();
        }
    }
}