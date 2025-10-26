using ProjectCI.Utilities.Runtime.Pools;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Projectiles
{
    /// <summary>
    /// Handles projectile behavior including curved movement and damage dealing
    /// </summary>
    public class PvMnVisualEffect : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private const int CheckEveryNFrame = 20;
        private static int _globalCheckingFrame = 0;

        private int _startFrame;
        [SerializeField] private bool isAutoEnded = true;

        public bool IsAutoEnded => isAutoEnded;

        private void Start()
        {
            if (_particleSystem) return;
            _particleSystem = GetComponent<ParticleSystem>();
            _startFrame = _globalCheckingFrame++;
        }

        private void Update()
        {
            if (!isAutoEnded)
            {
                return;
            }
            
            if (!_particleSystem)
            {
                return;
            }

            if ((Time.renderedFrameCount + _startFrame) % CheckEveryNFrame != 0)
            {
                return;
            }

            if (!_particleSystem.IsAlive(true))
            {
                MnObjectPool.Instance.Return(gameObject);
            }
        }
    }
} 