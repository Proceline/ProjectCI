using ProjectCI_Animation.Runtime;
using ProjectCI.CoreSystem.Runtime.Services;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Animation.Services
{
    [CreateAssetMenu(fileName = "PvSoAnimSetsCollection", menuName = "ProjectCI Tools/Animations/Services/PvSoAnimSetsCollection Service")]
    public class PvSoAnimSetsCollection : ScriptableObject, IService
    {
        [Header("Temporary Field for Animation")]
        public AnimationPlayableSupportBase defaultRiderAnimation;
        
        public void Dispose()
        {
            // Empty
        }

        public void Initialize()
        {
            // Empty
        }

        public void Cleanup()
        {
            // Empty
        }
    }
}