using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Deployment
{
    [CreateAssetMenu(fileName = "NewDeploymentController", menuName = "ProjectCI/Deployment/Deployment Controller", order = 1)]
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
