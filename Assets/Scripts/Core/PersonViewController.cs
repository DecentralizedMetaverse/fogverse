using R3;
using UnityEngine;

public class PersonViewController
{
    public static PersonViewController I { get; private set; }
    public Observable<CameraView> OnChangedCameraView => _onChangedCameraView;
    private readonly Subject<CameraView> _onChangedCameraView = new();

    public PersonViewController()
    {
        I = this;
    }

    public void SetCameraView(CameraView cameraView)
    {
        Debug.Log($"[FirstThirdPersonViewController] SetCameraView: {cameraView}");
        _onChangedCameraView.OnNext(cameraView);
    }
}
