using Teo.AutoReference;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    private static readonly int MinPriority = 0;
    private static readonly int MaxPriority = 100;

    [Get, SerializeField] private CameraManager _cameraManager;
    private CameraManager.CameraViewSettingsElement[] _vcams;

    // Cameraが1つかどうか
    public bool IsSingle => _vcams.Length == 1;

    /// <summary>
    /// カメラ切り替え
    /// </summary>
    /// <param name="view"></param>
    public void Set(CameraView view)
    {
        _vcams ??= _cameraManager.Vcams;

        _cameraManager.CurrentCameraView = view;
        foreach (var camera in _vcams)
        {
            if(camera.View == view)
            {
                camera.VirtualCamera.Priority = MaxPriority;
                if (camera.InputProvider != null) camera.InputProvider.enabled = true;
                _cameraManager.InputProvider = camera.InputProvider;
                continue;
            }
            camera.VirtualCamera.Priority = MinPriority;
            if (camera.InputProvider != null) camera.InputProvider.enabled = false;
        }
    }
}
