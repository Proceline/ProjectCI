using UnityEngine;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnExtraVisualInfoCanvas : MonoBehaviour
    {
        [SerializeField]
        private Animator roundEndAnimator;

        public void PlayAnimationWhileFriendRoundStarted()
        {
            roundEndAnimator.SetTrigger("Friendly");
        }

        public void PlayAnimationWhileEnemyRoundStarted()
        {
            roundEndAnimator.SetTrigger("Hostile");
        }
    }
}