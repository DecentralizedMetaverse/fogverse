using UnityEngine;
using UnityEngine.XR.Management;
using Cysharp.Threading.Tasks;
using MistNet;

public class VRDesktopSwitchLauncher : MonoBehaviour
{
    [SerializeField] private string desktopPrefabAddress;
    [SerializeField] private string vrPrefabAddress;

    private void Start()
    {
        var prefabAddress = "";
        // VRが有効かどうか
        prefabAddress = XRGeneralSettings.Instance.Manager.isInitializationComplete
            ? vrPrefabAddress
            : desktopPrefabAddress;
        MistManager.I.InstantiateAsync(prefabAddress, transform.position, transform.rotation).Forget();
    }
}
