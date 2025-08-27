using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Projectiles
{
    /// <summary>
    /// Handles projectile behavior including curved movement and damage dealing
    /// </summary>
    public class PvMnProjectile : MonoBehaviour
    {
        [SerializeField] internal string projectileTypeIdentifier;
        [SerializeField] private float speed;
        
        public UnityEvent<Vector3> onProjectileEnded;

        [Header("Curve Settings")]
        [SerializeField] private float maxHeight = 5f;
        [SerializeField] private float curveFactor = 1f;
        [SerializeField] private bool useCurve = true;
        [SerializeField] private float minDistance = 2f;     // 最小距离（低于此距离时开始降低曲线高度）
        [SerializeField] private float maxDistance = 10f;    // 最大距离（高于此距离时使用最大曲线高度）
        
        private Vector3 _direction;
        private bool _isInitialized;
        
        // Curve Information
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _journeyLength;
        private float _journeyTime;
        private float _journeyProgress;
        private float _adjustedMaxHeight;

        public bool IsProgressEnded => _journeyProgress >= 1f;
        public float ProgressDuration => _journeyTime;

        public void Initialize(Vector3 departure, Vector3 arrival)
        {
            transform.position = departure;
            _startPosition = departure;
            _targetPosition = arrival;
            _journeyLength = Vector3.Distance(_startPosition, _targetPosition);
            _journeyTime = _journeyLength / speed;
            _journeyProgress = 0f;

            float distanceRatio = Mathf.Clamp01((_journeyLength - minDistance) / (maxDistance - minDistance));
            _adjustedMaxHeight = maxHeight * distanceRatio;

            _direction = (_targetPosition - _startPosition).normalized;
            transform.rotation = Quaternion.LookRotation(_direction);

            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            if (_journeyProgress >= 1f)
            {
                _isInitialized = false;
                onProjectileEnded?.Invoke(_targetPosition);
                PvMnProjectilePool.CollectProjectile(this);
                return;
            }
            
            // Move Projectile
            _journeyProgress += Time.deltaTime / _journeyTime;
            
            if (useCurve)
            {
                Vector3 currentPosition = CalculateCurvedPosition(_journeyProgress);
                transform.position = currentPosition;
                
                if (_journeyProgress < 1f)
                {
                    Vector3 nextPosition = CalculateCurvedPosition(_journeyProgress + 0.01f);
                    Vector3 moveDirection = (nextPosition - currentPosition).normalized;
                    transform.rotation = Quaternion.LookRotation(moveDirection);
                }
            }
            else
            {
                // Move forward directly
                transform.position += _direction * (speed * Time.deltaTime);
            }
        }
        
        private Vector3 CalculateCurvedPosition(float progress)
        {
            // Calculate original position
            Vector3 basePosition = Vector3.Lerp(_startPosition, _targetPosition, progress);
            
            if (useCurve)
            {
                // Calculate height
                float heightOffset = Mathf.Sin(progress * Mathf.PI) * _adjustedMaxHeight * curveFactor;
                
                // Apply height
                basePosition.y += heightOffset;
            }
            
            return basePosition;
        }
    }
} 