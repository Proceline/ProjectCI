using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Services;

namespace ProjectCI.CoreSystem.Runtime.Services.Concrete
{
    [CreateAssetMenu(fileName = "NewServiceCenter", menuName = "ProjectCI/Services/Create ServiceCenter", order = 1)]
    public class SoFeLiteServiceCenter : ScriptableObject
    {
        [SerializeField]
        private ScriptableObject[] m_ServicesListBeforeSceneLoad;

        private static SoFeLiteServiceCenter m_Instance;

        /// <summary>
        /// Called when the game is loaded, before any scene is loaded
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnGameLoadedBeforeScene()
        {
            m_Instance = Resources.LoadAll<SoFeLiteServiceCenter>("")[0];
            foreach (var scriptableObject in m_Instance.m_ServicesListBeforeSceneLoad)
            {
                if (scriptableObject is IService service)
                {
                    ServiceLocator.Register(service);
                }
            }
        }
    }
} 