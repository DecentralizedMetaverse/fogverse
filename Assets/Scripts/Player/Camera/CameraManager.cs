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
    [GetInChildren] public CinemachineInputProvider InputProvider;
    [GetInChildren] public CinemachineVirtualCameraBase VirtualCamera;
    [GetInParent, SerializeField] private MistSyncObject _syncObject;

    private void Start()
    {
        if (!_syncObject.IsOwner) gameObject.SetActive(false);
        transform.SetParent(null);

        GM.Add<bool>("SetPauseCamera", SetPauseCamera);
        GM.Add("GetCurrentCameraView", () => CurrentCameraView);
    }

    private void SetPauseCamera(bool pause)
    {
        if (InputProvider == null) return;
        InputProvider.enabled = !pause;
    }
}
