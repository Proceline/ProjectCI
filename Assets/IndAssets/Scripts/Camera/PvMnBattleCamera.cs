using IndAssets.Scripts.Managers;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PvMnBattleCamera : MonoBehaviour
{
    [SerializeField] private Camera rotationPivotCam;
    [SerializeField] private float[] zoomBounds = new float[2] { 15f, 50f };

    [SerializeField] private Transform translateTarget;
    [SerializeField] private float movingSpeed;

    [SerializeField] private InputActionReference panDirectionActionRef;
    [NonSerialized] private InputAction _panDirectionInputAction;

    [SerializeField] private InputActionReference rotationActionRef;
    [SerializeField] private InputActionReference zoomInActionRef;
    [SerializeField] private InputActionReference zoomOutActionRef;

    [SerializeField] private UnityEvent<Camera> onRotationChanged;

    [SerializeField] private Vector3 boundCenter;
    [SerializeField] private Vector3 boundSize;
    [NonSerialized] private Bounds followingBounds;

    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private float defaultHorizontalAxis = 40f;
    [NonSerialized] private bool _inFollowMode;
    [NonSerialized] private Transform _followingObject;

    [SerializeField]
    private PvSoBattleState battleState;

    private Coroutine _movingCoroutine;

    private void Start()
    {
        followingBounds = new Bounds(boundCenter, boundSize);

        _panDirectionInputAction = panDirectionActionRef.ToInputAction();
        _panDirectionInputAction.Enable();

        rotationActionRef.ToInputAction().Enable();
        zoomInActionRef.ToInputAction().Enable();
        zoomOutActionRef.ToInputAction().Enable();
        zoomInActionRef.ToInputAction().performed += AssignCameraZoomIn;
        zoomOutActionRef.ToInputAction().performed += AssignCameraZoomOut;

        battleState.RegisterCallbackOnEnter(ApplyCameraOnStateSwitched);
    }

    private void OnDestroy()
    {
        _panDirectionInputAction.Disable();

        rotationActionRef.ToInputAction().Disable();
        zoomInActionRef.ToInputAction().Disable();
        zoomOutActionRef.ToInputAction().Disable();

        zoomInActionRef.ToInputAction().performed -= AssignCameraZoomIn;
        zoomOutActionRef.ToInputAction().performed -= AssignCameraZoomOut;

        battleState.UnregisterCallbackOnEnter(ApplyCameraOnStateSwitched);
    }

    void Update()
    {
        if (_inFollowMode)
        {
            translateTarget.position = _followingObject.position;
        }
        else
        {
            var direction = _panDirectionInputAction.ReadValue<Vector2>();
            var directionPivotTarget = rotationPivotCam.transform;
            var moveDir = directionPivotTarget.right * direction.x + directionPivotTarget.up * direction.y;
            moveDir.y = 0;

            var dirInFrame = moveDir * movingSpeed * Time.deltaTime;

            var nextPosition = translateTarget.position + dirInFrame;
            if (followingBounds.Contains(nextPosition))
            {
                translateTarget.Translate(dirInFrame, Space.Self);
            }
        }
        
        if (rotationActionRef.action.enabled)
        {
            UpdateCameraHorizontalAxisForRotation(rotationActionRef.action);
        }
    }

    /// <summary>
    /// Binded into AI Manager so when select AI Unit camera will follow
    /// </summary>
    /// <param name="targetUnit"></param>
    public void FollowOnUnit(PvMnBattleGeneralUnit targetUnit)
    {
        translateTarget.position = targetUnit.transform.position;
        _followingObject = targetUnit.transform;
        _inFollowMode = true;
    }

    /// <summary>
    /// Binded into AI Manager so when all thoughts finished
    /// </summary>
    public void UnfollowTransform()
    {
        if (_movingCoroutine != null)
        {
            StopCoroutine(_movingCoroutine);
            _movingCoroutine = null;
        }

        _inFollowMode = false;
        _followingObject = null;
    }

    private void ApplyCameraOnStateSwitched(PvPlayerRoundState inState, PvMnBattleGeneralUnit targetUnit)
    {
        if (inState == PvPlayerRoundState.None)
        {
            UnfollowTransform();
        }
        else if (inState == PvPlayerRoundState.Moving)
        {
            if (_movingCoroutine != null)
            {
                StopCoroutine(_movingCoroutine);
            }
            _movingCoroutine = StartCoroutine(TranslateCameraToPosition(translateTarget.position, 
                targetUnit.transform.position, 0.15f, () => FollowOnUnit(targetUnit)));
        }
        else if (inState == PvPlayerRoundState.Selected || inState == PvPlayerRoundState.Prepare)
        {
            translateTarget.position = targetUnit.transform.position;
            UnfollowTransform();
        }
    }

    private IEnumerator TranslateCameraToPosition(Vector3 currentCenter, Vector3 targetPosition, float duration, Action onFinished)
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

        onFinished.Invoke();
        _movingCoroutine = null;
    }


    #region Manual Camera Control

    private void AssignCameraZoomIn(InputAction.CallbackContext context)
    {
        if (PvMnGameController.IsControllerLocked)
        {
            return;
        }

        if (orbitalFollow.TargetOffset.y > (zoomBounds[1] - 0.5f))
        {
            return;
        }

        orbitalFollow.TargetOffset += Vector3.down;
    }

    private void AssignCameraZoomOut(InputAction.CallbackContext context)
    {
        if (PvMnGameController.IsControllerLocked)
        {
            return;
        }

        if (orbitalFollow.TargetOffset.y < (zoomBounds[0] + 0.5f))
        {
            return;
        }

        orbitalFollow.TargetOffset += Vector3.up;
    }

    private void UpdateCameraHorizontalAxisForRotation(InputAction inputAction)
    {
        if (PvMnGameController.IsControllerLocked)
        {
            return;
        }

        var value = inputAction.ReadValue<float>();
        if (Mathf.Approximately(value, 0))
        {
            return;
        }
        onRotationChanged.Invoke(rotationPivotCam);
        orbitalFollow.HorizontalAxis.Value -= value;
    }

    #endregion
}
