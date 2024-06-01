using Cinemachine;
using Teo.AutoReference;
using UnityEngine;
using UnityEngine.InputSystem;
using R3;

/// <summary>
/// [Mobile][Desktop] Camera Zoom
/// NOTE: カメラの近くに壁があるときは、壁にあたって距離が変わらないように見えるので注意
/// </summary>
internal class CameraZoom : MonoBehaviour
{
    private const float ThirdPersonViewMaxDistance = 10.0f;
    private const float ThirdPersonViewMinDistance = 0.5f; // Cameraを切り替える閾値としても使用する
    private const float ThirdPersonViewScrollDivide = 1.0f / 360f; // Scrollの感度

    private const float FirstPersonViewScrollDivide = 0.0005f; // Scrollの感度
    private const float FirstPersonViewMaxVerticalFOV = 60.0f;
    private const float FirstPersonViewMinVerticalFOV = 1.0f;

    private const float Damping = 1.0f;

    [Get, SerializeField] private CameraManager _cameraManager;

    private CinemachineInputProvider _inputProvider;
    private CinemachineFramingTransposer _framingTransposer; // 距離変更用
    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    private bool _isMoving;

    private void Start()
    {
        InputF.action.Game.Scroll.performed += OnScroll;

        SetFramingTransposer(_cameraManager.VirtualCamera);

        PersonViewController.I.OnChangedCameraView.Subscribe(SetCameraView).AddTo(this);
    }

    private void SetCameraView(CameraView cameraView)
    {
        if (cameraView == CameraView.ThirdPerson)
        {
            _cinemachineVirtualCamera.m_Lens.FieldOfView = FirstPersonViewMaxVerticalFOV;
            _framingTransposer.m_CameraDistance = ThirdPersonViewMinDistance + 0.1f; // 少し距離を戻してから距離変更に移行
            SetDamping(Damping);
        }
        else
        {
            _framingTransposer.m_CameraDistance = 0;
            SetFirstPersonViewFOV(0);
            SetDamping(0);
        }
    }

    private void SetDamping(float value)
    {
        _framingTransposer.m_XDamping = value;
        _framingTransposer.m_YDamping = value;
        _framingTransposer.m_ZDamping = value;
    }

    private void OnDestroy()
    {
        InputF.action.Game.Scroll.performed -= OnScroll;
    }

    private void SetFramingTransposer(CinemachineVirtualCameraBase vcam)
    {
        _cinemachineVirtualCamera = vcam as CinemachineVirtualCamera;
        var cameraComponent = _cinemachineVirtualCamera?.GetCinemachineComponent(CinemachineCore.Stage.Body);
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
        var distance = _framingTransposer.m_CameraDistance;

        if (distance > ThirdPersonViewMinDistance)
        {
            SetThirdPersonViewDistance(value);
        }
        else
        {
            SetFirstPersonViewFOV(value);
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

        // 距離が0になったらFOVを変更する
        if (distance <= ThirdPersonViewMinDistance)
        {
            PersonViewController.I.SetCameraView(CameraView.FirstPerson);
        }
    }

    /// <summary>
    /// NOTE: 厳密にはFOVを変更する
    /// </summary>
    /// <param name="addValue"></param>
    private void SetFirstPersonViewFOV(float addValue)
    {
        if (_cinemachineVirtualCamera == null) return;

        var fov = _cinemachineVirtualCamera.m_Lens.FieldOfView;
        // FOVが小さくなるほど変更量が小さくなるように調整
        var adjustmentFactor = fov * FirstPersonViewScrollDivide;
        fov -= addValue * adjustmentFactor;
        fov = Mathf.Clamp(fov, FirstPersonViewMinVerticalFOV, FirstPersonViewMaxVerticalFOV + 1); // 初期値が60であり、下の条件で3人称に切り替わってしまうため+1する
        _cinemachineVirtualCamera.m_Lens.FieldOfView = fov;

        // FOVが最大値に達したら距離による変更に戻る
        if (fov > FirstPersonViewMaxVerticalFOV)
        {
            PersonViewController.I.SetCameraView(CameraView.ThirdPerson);
        }
    }
}
