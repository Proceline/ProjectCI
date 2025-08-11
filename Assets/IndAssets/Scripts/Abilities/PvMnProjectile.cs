using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Projectiles
{
    /// <summary>
    /// Handles projectile behavior including curved movement and damage dealing
    /// </summary>
    public class PvMnProjectile : MonoBehaviour
    {
        [Header("Basic Settings")]
        [SerializeField] private GameObject hitEffect;    // 命中特效预制体
        [SerializeField] private float speed;

        [Header("Curve Settings")]
        [SerializeField] private float maxHeight = 5f;        // 最大高度
        [SerializeField] private float curveFactor = 1f;     // 曲线程度
        [SerializeField] private bool useCurve = true;       // 是否使用曲线
        [SerializeField] private float minDistance = 2f;     // 最小距离（低于此距离时开始降低曲线高度）
        [SerializeField] private float maxDistance = 10f;    // 最大距离（高于此距离时使用最大曲线高度）
        
        private Vector3 _direction;
        private bool _isInitialized;
        
        // 曲线运动相关
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _journeyLength;
        private float _journeyTime;
        private float _journeyProgress;
        private float _adjustedMaxHeight;  // 根据距离调整后的最大高度

        public void Initialize(Vector3 departure, Vector3 arrival)
        {
            transform.position = departure;
            _startPosition = departure;
            _targetPosition = arrival;
            _journeyLength = Vector3.Distance(_startPosition, _targetPosition);
            _journeyTime = _journeyLength / speed;
            _journeyProgress = 0f;

            // 根据距离调整最大高度
            float distanceRatio = Mathf.Clamp01((_journeyLength - minDistance) / (maxDistance - minDistance));
            _adjustedMaxHeight = maxHeight * distanceRatio;

            // 计算基础方向
            _direction = (_targetPosition - _startPosition).normalized;
            transform.rotation = Quaternion.LookRotation(_direction);

            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized) 
            {
                Destroy(gameObject);
                return;
            }
            
            // 更新进度
            _journeyProgress += Time.deltaTime / _journeyTime;
            
            if (useCurve)
            {
                // 使用曲线计算位置
                Vector3 currentPosition = CalculateCurvedPosition(_journeyProgress);
                transform.position = currentPosition;
                
                // 更新朝向（朝向移动方向）
                if (_journeyProgress < 1f)
                {
                    Vector3 nextPosition = CalculateCurvedPosition(_journeyProgress + 0.01f);
                    Vector3 moveDirection = (nextPosition - currentPosition).normalized;
                    transform.rotation = Quaternion.LookRotation(moveDirection);
                }
            }
            else
            {
                // 直线移动
                transform.position += _direction * speed * Time.deltaTime;
            }
        }
        
        private Vector3 CalculateCurvedPosition(float progress)
        {
            // 计算基础位置
            Vector3 basePosition = Vector3.Lerp(_startPosition, _targetPosition, progress);
            
            if (useCurve)
            {
                // 计算高度偏移（使用调整后的最大高度）
                float heightOffset = Mathf.Sin(progress * Mathf.PI) * _adjustedMaxHeight * curveFactor;
                
                // 应用高度偏移
                basePosition.y += heightOffset;
            }
            
            return basePosition;
        }
    }
} 