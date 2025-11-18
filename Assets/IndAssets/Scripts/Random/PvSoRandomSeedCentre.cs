using ProjectCI.CoreSystem.Runtime.Services;
using UnityEngine;

namespace IndAssets.Scripts.Random
{
    using Random = UnityEngine.Random;
    
    [CreateAssetMenu(fileName = "PvSoRandomSeedCentre", menuName = "Scriptable Objects/PvSoRandomSeedCentre")]
    public class PvSoRandomSeedCentre : ScriptableObject, IService
    {
        public int GetNextRandomNumber(int fromValueIncl, int toValueIncl)
        {
            return Random.Range(0, 100000) % (toValueIncl - fromValueIncl) + fromValueIncl;
        }

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
