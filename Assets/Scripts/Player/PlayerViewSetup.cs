using R3;
using StarterAssets;
using Teo.AutoReference;
using UnityEngine;

public class PlayerViewSetup : MonoBehaviour
{
    [GetInChildren, Name("Face"), SerializeField] private Transform face;
    [Get, SerializeField] private MovementController movementController;

    private void Start()
    {
        PersonViewController.I.OnChangedCameraView.Subscribe(OnChangedCameraView).AddTo(this);
    }

    private void OnChangedCameraView(CameraView cameraView)
    {
        switch (cameraView)
        {
            case CameraView.FirstPerson:
                face.gameObject.SetActive(false);
                movementController.IsFirstPerson = true;
                break;
            case CameraView.ThirdPerson:
                face.gameObject.SetActive(true);
                movementController.IsFirstPerson = false;
                break;
        }
    }
}
