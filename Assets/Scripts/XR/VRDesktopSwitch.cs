using UnityEngine;
using UnityEngine.XR.Management;
using Cysharp.Threading.Tasks;
using UnityEngine.XR;

public class VRDesktopSwitch : MonoBehaviour
{
    [SerializeField] private GameObject desktop;
    [SerializeField] private GameObject vr;

    private XRInputSubsystem xrInputSubsystem;

    private void Start()
    {
        InputF.action.Common.SwitchVR.performed += OnSwitchVR;
        InitializeXR().Forget();
    }

    private void OnDestroy()
    {
        InputF.action.Common.SwitchVR.performed -= OnSwitchVR;

        if (xrInputSubsystem != null)
        {
            xrInputSubsystem.trackingOriginUpdated -= OnDeviceChange;
            xrInputSubsystem.trackingOriginUpdated -= OnDeviceChange;
        }
    }

    private void OnSwitchVR(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ToggleXRMode().Forget();
    }

    private async UniTaskVoid ToggleXRMode()
    {
        if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            StopXR();
        }
        else
        {
            await StartXR();
        }
    }

    private async UniTask InitializeXR()
    {
        await XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("XRローダーの初期化に失敗しました。");
            return;
        }

        xrInputSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRInputSubsystem>();

        if (xrInputSubsystem != null)
        {
            xrInputSubsystem.trackingOriginUpdated += OnDeviceChange;
            xrInputSubsystem.trackingOriginUpdated += OnDeviceChange;
        }

        if (XRGeneralSettings.Instance.Manager.isInitializationComplete && xrInputSubsystem != null && xrInputSubsystem.running)
        {
            vr.SetActive(true);
            desktop.SetActive(false);
        }
        else
        {
            vr.SetActive(false);
            desktop.SetActive(true);
        }
    }

    private async UniTask StartXR()
    {
        await XRGeneralSettings.Instance.Manager.InitializeLoader();
        XRGeneralSettings.Instance.Manager.StartSubsystems();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("XRローダーの初期化に失敗しました。");
            return;
        }

        XRGeneralSettings.Instance.Manager.StartSubsystems();

        desktop.SetActive(false);
        vr.SetActive(true);
    }

    private void StopXR()
    {
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();

        desktop.SetActive(true);
        vr.SetActive(false);
    }

    private void OnDeviceChange(XRInputSubsystem obj)
    {
        if (obj.running)
        {
            StartXR().Forget();
        }
        else
        {
            StopXR();
        }
    }

    public void OnApplicationQuit() {
        if (XRGeneralSettings.Instance.Manager.activeLoader != null) {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}
