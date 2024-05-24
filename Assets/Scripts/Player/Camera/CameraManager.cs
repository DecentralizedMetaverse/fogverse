using System;
using Cinemachine;
using System.Linq;
using DC;
using MistNet;
using Teo.AutoReference;
using UnityEngine;

/// <summary>
/// vcamの2番目はEquipment用のCameraが入る
/// </summary>
public class CameraManager : MonoBehaviour
{
    public CameraView CurrentCameraView;
    public CameraViewSettingsElement[] Vcams;
    public CinemachineInputProvider InputProvider { get; set; }
    [GetInParent, SerializeField] private MistSyncObject _syncObject;
    [Get, SerializeField] private CameraSwitch _cameraSwitch;

    [Serializable]
    public class CameraViewSettingsElement
    {
        public CameraView View;
        public CinemachineVirtualCameraBase VirtualCamera;
        public CinemachineInputProvider InputProvider;
    }

    private void Start()
    {
        if(!_syncObject.IsOwner) gameObject.SetActive(false);
        transform.SetParent(null);

        GM.Add<bool>("SetPauseCamera", SetPauseCamera);
        GM.Add("GetCurrentCameraView", ()=>CurrentCameraView);

        if (GM.mng.Device == DeviceMode.VR)
        {
            // TODO: 装備画面から戻る際にVR画面に戻さないといけない
            _cameraSwitch.Set(CameraView.VR);
        }

        InputProvider = Vcams.Where(x => x.View == CurrentCameraView).Select(x => x.InputProvider).FirstOrDefault();
        _cameraSwitch.Set(CurrentCameraView);
    }

    private void SetPauseCamera(bool pause)
    {
        if (InputProvider == null) return;
        InputProvider.enabled = !pause;
    }
}
