using System;
using System.Linq;
using Cinemachine;
using DC;
using Teo.AutoReference;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [Mobile][Desktop] Camera Zoom
/// NOTE: カメラの近くに壁があるときは、壁にあたって距離が変わらないように見えるので注意
/// </summary>
internal class CameraZoom : MonoBehaviour
{
    private static readonly float ThirdPersonViewMaxDistance = 10.0f;
    private static readonly float ThirdPersonViewMinDistance = 0.5f; // Cameraを切り替える閾値としても使用する
    private static readonly float ThirdPersonViewScrollDivide = 1.0f / 360f; // Scrollの感度

    private static readonly float FirstPersonViewScrollDivide = 0.0005f; // Scrollの感度
    private static readonly float FirstPersonViewMaxVerticalFOV = 60.0f;
    private static readonly float FirstPersonViewMinVerticalFOV = 1.0f;

    [Get, SerializeField] private CameraManager _cameraManager;
    [Get, SerializeField] private CameraSwitch _cameraSwitch;

    private CinemachineInputProvider _inputProvider;
    private CinemachineFramingTransposer _framingTransposer; // 距離変更用
    private bool _isMoving;
    private CameraManager.CameraViewSettingsElement[] _vcams;

    private void Start()
    {
        InputF.action.Game.Scroll.performed += OnScroll;
        _vcams = _cameraManager.Vcams;
        SetFramingTransposer(_vcams.FirstOrDefault(x => x.View == CameraView.ThirdPerson)?.VirtualCamera);
    }

    private void OnDestroy()
    {
        InputF.action.Game.Scroll.performed -= OnScroll;
    }

    private void SetFramingTransposer(CinemachineVirtualCameraBase vcam)
    {
        var cameraComponent =
            (vcam as CinemachineVirtualCamera)?.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (cameraComponent is CinemachineFramingTransposer component)
        {
            _framingTransposer = component;
        }
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        // Cameraの距離を変更
        var value = context.ReadValue<float>();
        SetZoom(value);
    }

    private void SetZoom(float value)
    {
        if (_cameraManager.CurrentCameraView == CameraView.FirstPerson)
        {
            SetFirstPersonViewDistance(value);
        }
        else if (_cameraManager.CurrentCameraView == CameraView.ThirdPerson)
        {
            SetThirdPersonViewDistance(value);
        }
    }

    private void SetThirdPersonViewDistance(float addValue)
    {
        if (_framingTransposer == null) return;

        var distance = _framingTransposer.m_CameraDistance;
        distance -= addValue * ThirdPersonViewScrollDivide;
        distance = Mathf.Clamp(distance, ThirdPersonViewMinDistance, ThirdPersonViewMaxDistance);
        Debug.Log(distance);
        _framingTransposer.m_CameraDistance = distance;

        // 距離によってCameraを切り替える
        if (!_cameraSwitch.IsSingle && distance <= ThirdPersonViewMinDistance)
        {
            _cameraSwitch.Set(CameraView.FirstPerson);
        }
    }

    /// <summary>
    /// NOTE: 厳密にはFOVを変更する
    /// </summary>
    /// <param name="addValue"></param>
    private void SetFirstPersonViewDistance(float addValue)
    {
        var virtualCamera = _vcams.FirstOrDefault(x => x.View == CameraView.FirstPerson)?.VirtualCamera;
        var cinemachineVirtualCamera = virtualCamera as CinemachineVirtualCamera;
        if (cinemachineVirtualCamera == null) return;

        var fov = cinemachineVirtualCamera.m_Lens.FieldOfView;
        // FOVが小さくなるほど変更量が小さくなるように調整
        var adjustmentFactor = fov * FirstPersonViewScrollDivide;
        fov -= addValue * adjustmentFactor;
        fov = Mathf.Clamp(fov, FirstPersonViewMinVerticalFOV,
            FirstPersonViewMaxVerticalFOV + 1); // 初期値が60であり、下の条件で3人称に切り替わってしまうため+1する
        cinemachineVirtualCamera.m_Lens.FieldOfView = fov;

        // 距離によってCameraを切り替える
        if (fov > FirstPersonViewMaxVerticalFOV)
        {
            _cameraSwitch.Set(CameraView.ThirdPerson);
        }
    }
}
