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

    [NonSerialized] private float _currentZoomValue;

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

    private void Start()
    {
        _currentZoomValue = 0;
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
    
    private void AssignCameraZoomIn(InputAction.CallbackContext context)
    {
        var zoomDelta = zoomingSpeed * context.ReadValue<float>();
        AddOnCameraZoom(zoomDelta);
    }
    
    private void AssignCameraZoomOut(InputAction.CallbackContext context)
    {
        var zoomDelta = -zoomingSpeed * context.ReadValue<float>();
        AddOnCameraZoom(zoomDelta);
    }

    private void AddOnCameraZoom(float zoomDelta)
    {
        _currentZoomValue += zoomDelta;
        var moveDir = transform.forward * zoomDelta;
        translateTarget.Translate(moveDir, Space.Self);
    }

    private IEnumerator AssignCameraZoom(float targetZoomValue, float duration)
    {
        float zoomDelta;
        if (targetZoomValue > 0)
        {
            zoomDelta = targetZoomValue > _currentZoomValue ? targetZoomValue - _currentZoomValue : _currentZoomValue;
        }
        else
        {
            zoomDelta = targetZoomValue < _currentZoomValue ? targetZoomValue - _currentZoomValue : _currentZoomValue;
        }

        float deltaSum = 0;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(elapsed / duration);
            var deltaProgress = Mathf.Lerp(0f, zoomDelta, progress);
            var delta = deltaProgress - deltaSum;
            deltaSum = deltaProgress;
            AddOnCameraZoom(delta);
            yield return null;
        }
    }

    private IEnumerator TranslateCameraToPosition(Vector3 currentCenter, Vector3 targetPosition, float duration)
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

    private void AssignCameraZoom(float targetZoomValue)
    {
        float zoomDelta;
        if (targetZoomValue > 0)
        {
            zoomDelta = targetZoomValue > _currentZoomValue ? targetZoomValue - _currentZoomValue : _currentZoomValue;
        }
        else
        {
            zoomDelta = targetZoomValue < _currentZoomValue ? targetZoomValue - _currentZoomValue : _currentZoomValue;
        }
        
        AddOnCameraZoom(zoomDelta);
    }

    private void AssignCameraRotation(InputAction.CallbackContext context)
    {
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
                case UnitBattleState.UsingAbility:
                case UnitBattleState.AbilityTargeting:
                    var zoomValue = state == UnitBattleState.UsingAbility ? 20 : 0;
                    var duration = 0.25f;
                    if (GetCurrentCenter(out var center, out _))
                    {
                        StartCoroutine(TranslateCameraToPosition(center, owner.Position, duration));
                    }
                    StartCoroutine(AssignCameraZoom(zoomValue, duration));
                    break;
                case UnitBattleState.AbilityConfirming:
                case UnitBattleState.Idle:
                case UnitBattleState.MovingProgress:
                case UnitBattleState.Finished:
                default:
                    AssignCameraZoom(0);
                    break;
            }
        }
        else
        {
            AssignCameraZoom(0);
        }
    }
}
