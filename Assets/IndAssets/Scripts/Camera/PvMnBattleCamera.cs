using System;
using System.Collections;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Runtime.GUI.Battle;
using ProjectCI.Utilities.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PvMnBattleCamera : MonoBehaviour
{
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;
    [SerializeField] private float zoomingSpeed;

    /// <summary>
    /// Used to record AUTO-ZOOM
    /// </summary>
    [NonSerialized] private float _currentZoomValue;
    
    /// <summary>
    /// Used to record Manual-ZOOM
    /// </summary>
    [NonSerialized] private float _zoomValueAdjustor;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Camera rotationPivotCam;
    [SerializeField] private float rotatingSpeed;

    [SerializeField] private Transform translateTarget;
    [SerializeField] private float movingSpeed;

    [SerializeField] private InputActionReference panDirectionActionRef;
    [NonSerialized] private InputAction _panDirectionInputAction;
    
    [SerializeField] private InputActionReference rotationActionRef;
    [SerializeField] private InputActionReference zoomInActionRef;
    [SerializeField] private InputActionReference zoomOutActionRef;

    [SerializeField] private UnityEvent<Camera> onRotationChanged;
    
    [SerializeField] 
    private PvSoUnitBattleStateEvent onStateDetermined;

    [SerializeField] 
    private UnityEvent<bool> onCameraStartOrEndAnyTween;

    [NonSerialized] private Coroutine _cameraZoomingCoroutine;

    private void Start()
    {
        _currentZoomValue = 0;
        _zoomValueAdjustor = 0;
        _panDirectionInputAction = panDirectionActionRef.ToInputAction();
        _panDirectionInputAction.Enable();
        
        rotationActionRef.ToInputAction().Enable();
        rotationActionRef.ToInputAction().performed += AssignCameraRotation;
        
        zoomInActionRef.ToInputAction().Enable();
        zoomOutActionRef.ToInputAction().Enable();
        zoomInActionRef.ToInputAction().performed += AssignCameraZoomIn;
        zoomOutActionRef.ToInputAction().performed += AssignCameraZoomOut;
        
        onStateDetermined.RegisterCallback(ApplyCameraChangeOnStateDetermined);
    }

    private void OnDestroy()
    {
        _currentZoomValue = 0;
        _panDirectionInputAction.Disable();
        
        rotationActionRef.ToInputAction().Disable();
        zoomInActionRef.ToInputAction().Disable();
        zoomOutActionRef.ToInputAction().Disable();
        
        rotationActionRef.ToInputAction().performed -= AssignCameraRotation;
        zoomInActionRef.ToInputAction().performed -= AssignCameraZoomIn;
        zoomOutActionRef.ToInputAction().performed -= AssignCameraZoomOut;
        
        onStateDetermined.UnregisterCallback(ApplyCameraChangeOnStateDetermined);
    }

    void Update()
    {
        var direction = _panDirectionInputAction.ReadValue<Vector2>();
        var moveDir = transform.right * direction.x + transform.up * direction.y;
        moveDir.y = 0;
        
        translateTarget.Translate(moveDir * (movingSpeed * Time.deltaTime), Space.Self);
    }

    private void AddOnCameraZoom(float zoomDelta, ref float recordValue)
    {
        recordValue += zoomDelta;
        var moveDir = transform.forward * zoomDelta;
        translateTarget.Translate(moveDir, Space.Self);
    }

    private void SetRotationWithDegrees(float degrees)
    {
        if (!GetCurrentCenter(out var center, out var groundNormal))
        {
            return;
        }
        
        Vector3 centerAtObjHeight = new Vector3(center.x, translateTarget.position.y, center.z);
        var rel = Vector3.ProjectOnPlane(translateTarget.position - centerAtObjHeight, groundNormal);
        Quaternion rot = Quaternion.AngleAxis(degrees, groundNormal);
        Vector3 newRel = rot * rel;

        translateTarget.position = centerAtObjHeight + newRel;
        
        // Make sure camera face the ground center
        transform.LookAt(center, groundNormal);
        onRotationChanged?.Invoke(rotationPivotCam);
        
        // TODO: Refactor UI Part
        var allBarContainers = FindObjectsByType<PvMnBattleResourceContainer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var barContainer in allBarContainers)
        {
            barContainer.RotateHealthBarByCamera(transform);
        }
        
    }

    private bool GetCurrentCenter(out Vector3 center, out Vector3 groundNormal)
    {
        Ray ray = rotationPivotCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        var defaultGroundNormal = Vector3.up;
        center = default;
        groundNormal = defaultGroundNormal;

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f, groundLayer, QueryTriggerInteraction.Ignore))
        {
            center = hit.point;
            groundNormal = hit.normal;
        }
        else
        {
            // If not Hit ground, use Y=0
            Plane plane = new Plane(defaultGroundNormal, Vector3.zero);
            if (plane.Raycast(ray, out float t))
                center = ray.GetPoint(t);
            else
                return false;
        }

        return true;
    }

    private void ApplyCameraChangeOnStateDetermined(IEventOwner owner, UnitStateEventParam stateEventParam)
    {
        if (owner is not PvMnBattleGeneralUnit) return;
        var state = stateEventParam.battleState;
        var stateBehaviour = stateEventParam.behaviour;

        if (stateBehaviour == UnitStateBehaviour.Adding)
        {
            switch (state)
            {
                case UnitBattleState.Moving:
                    StartToMoveCamera(owner.Position, 0f);
                    break;
                case UnitBattleState.UsingAbility:
                case UnitBattleState.AbilityTargeting:
                    StartToMoveCamera(owner.Position, 20f);
                    break;
                case UnitBattleState.AbilityConfirming:
                    StartToMoveCamera(owner.Position, -10f, 0.1f);
                    break;
                case UnitBattleState.Idle:
                case UnitBattleState.MovingProgress:
                case UnitBattleState.Finished:
                default:
                    StartToMoveCamera(owner.Position, 0f, 0.1f);
                    break;
            }
        }
        else
        {
            StartToMoveCamera(owner.Position, 0f, 0.1f);
        }
    }

    private void StartToMoveCamera(Vector3 position, float zoomValue, float duration = 0.25f)
    {
        if (GetCurrentCenter(out var center, out _))
        {
            StartCoroutine(TranslateCameraToPosition(center, position, duration));
        }

        _cameraZoomingCoroutine ??= StartCoroutine(AssignCameraZoom(zoomValue, duration));
    }

    #region Camera Tween

    private IEnumerator AssignCameraZoom(float targetZoomValue, float duration)
    {
        if (targetZoomValue > 0 && _zoomValueAdjustor >= targetZoomValue)
        {
            yield break;
        }

        if (Mathf.Approximately(targetZoomValue, _currentZoomValue))
        {
            yield break;
        }

        onCameraStartOrEndAnyTween.Invoke(true);
        {
            var zoomDelta = targetZoomValue - _currentZoomValue;
            float deltaSum = 0;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                var deltaProgress = Mathf.Lerp(0f, zoomDelta, progress);
                var delta = deltaProgress - deltaSum;
                deltaSum = deltaProgress;
                AddOnCameraZoom(delta, ref _currentZoomValue);
                yield return null;
            }
        }
        onCameraStartOrEndAnyTween.Invoke(false);
        _cameraZoomingCoroutine = null;
    }

    private IEnumerator TranslateCameraToPosition(Vector3 currentCenter, Vector3 targetPosition, float duration)
    {
        onCameraStartOrEndAnyTween.Invoke(true);
        {
            var deltaDirection = targetPosition - currentCenter;
            deltaDirection.y = 0;

            var directionSum = Vector3.zero;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                var deltaProgress = Vector3.Lerp(Vector3.zero, deltaDirection, progress);
                var delta = deltaProgress - directionSum;
                directionSum = deltaProgress;
                translateTarget.Translate(delta, Space.Self);
                yield return null;
            }
        }
        onCameraStartOrEndAnyTween.Invoke(false);
    }
    #endregion
    
    #region Manual Camera Control

    private void AssignCameraZoomIn(InputAction.CallbackContext context)
    {
        if (!FeLiteGameController.IsBasicControllerEnabled)
        {
            return;
        }

        if (_zoomValueAdjustor > maxZoom)
        {
            return;
        }

        var zoomDelta = zoomingSpeed * context.ReadValue<float>();
        AddOnCameraZoom(zoomDelta, ref _zoomValueAdjustor);
    }

    private void AssignCameraZoomOut(InputAction.CallbackContext context)
    {
        if (!FeLiteGameController.IsBasicControllerEnabled)
        {
            return;
        }
        
        if (_zoomValueAdjustor < minZoom)
        {
            return;
        }

        var zoomDelta = -zoomingSpeed * context.ReadValue<float>();
        AddOnCameraZoom(zoomDelta,  ref _zoomValueAdjustor);
    }
    
    private void AssignCameraRotation(InputAction.CallbackContext context)
    {
        if (!FeLiteGameController.IsBasicControllerEnabled)
        {
            return;
        }

        var value = context.ReadValue<float>();
        switch (value)
        {
            case > 0:
                SetRotationWithDegrees(45);
                break;
            case < 0:
                SetRotationWithDegrees(-45);
                break;
        }
    }
    #endregion
}
