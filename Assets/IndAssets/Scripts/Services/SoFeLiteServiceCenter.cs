using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Services.Concrete
{
    [CreateAssetMenu(fileName = "NewServiceCenter", menuName = "ProjectCI/Services/Create ServiceCenter", order = 1)]
    public class SoFeLiteServiceCenter : ScriptableObject
    {
        [SerializeField]
        private ScriptableObject[] m_ServicesListBeforeSceneLoad;

        [SerializeField] private UnityEvent onAfterSceneLoadEventRaised;

        private static SoFeLiteServiceCenter _instance;

        /// <summary>
        /// Called when the game is loaded, before any scene is loaded
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnGameLoadedBeforeScene()
        {
            _instance = Resources.LoadAll<SoFeLiteServiceCenter>("")[0];
            foreach (var scriptableObject in _instance.m_ServicesListBeforeSceneLoad)
            {
                if (scriptableObject is IService service)
                {
                    ServiceLocator.Register(service);
                }
            }
        }
        
        /// <summary>
        /// Called right after Scene Loaded
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnGameLoadedAfterScene()
        {
            _instance.onAfterSceneLoadEventRaised?.Invoke();
        }
    }
} 