using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera2D : MonoBehaviour
{
    [SerializeField] GameObject virtualCamera;

    void Start()
    {
        //GM.Add<bool>("EnablePlayerCamera2D", (enable) =>
        //{
        //    virtualCamera.SetActive(enable);
        //});

        InputF.action.Game.ChangeCamera.started += OnCameraChanged;
    }

    private void OnCameraChanged(InputAction.CallbackContext context)
    {
        virtualCamera.SetActive(!virtualCamera.activeSelf);
    }
}
