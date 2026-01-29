using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Deployment
{
    public class PvMnDeploymentBridge : MonoBehaviour
    {
        [SerializeField]
        private PvSoDeploymentController deploymentController;

        [SerializeField]
        private UnityEvent onBattleSceneLaunched;

        private void Awake()
        {
            deploymentController.ShowAllSpawnHints();
        }

        private void Start()
        {
            deploymentController.PutCameraOnPositionOnStarted();
            onBattleSceneLaunched?.Invoke();
        }
    }
}
