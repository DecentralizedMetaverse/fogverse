using DC;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// àÍêlèÃCamera
/// </summary>
[Obsolete]
public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 1.0f;
    [SerializeField] public float cameraSpeed = 1.0f;
    [SerializeField] int limitAngle = 45;
    private Vector2 moveVector;

    void Start()
    {
        InputF.action.Game.Move.started += OnMove;
        InputF.action.Game.Move.performed += OnMove;
        InputF.action.Game.Move.canceled += OnMove;

        //InputF.action.Game.Camera.started += OnCamera;
        //InputF.action.Game.Camera.performed += OnCamera;
        //InputF.action.Game.Camera.canceled += OnCamera;

        InputF.action.Game.Cancel.performed += OnCancel;
        
    }

    void Update()
    {
        LimitAngle();
        var forward = moveVector * moveSpeed * Time.deltaTime;
        //if (InputF.action.Game.Dash.IsPressed())
        {
            forward *= 2.0f;
        }
        transform.position += CalcMovementFromCamera(forward);
    }

    private void OnCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("OnCancel");
        GM.pause = GM.pause == ePause.mode.none?
            ePause.mode.GameStop:
            ePause.mode.none;
    }    

    private void OnMove(InputAction.CallbackContext contex)
    {
        if (GM.pause != ePause.mode.none) return;
        moveVector = contex.ReadValue<Vector2>();
    }    
    
    private void OnCamera(InputAction.CallbackContext contex)
    {
        if (GM.pause != ePause.mode.none) return;

        var rotateVec = contex.ReadValue<Vector2>();
        rotateVec *= cameraSpeed;
        var rot = transform.rotation.eulerAngles;
        rot.x -= rotateVec.y;
        rot.y += rotateVec.x;
        transform.localRotation = Quaternion.Euler(rot);
    }

    /// <summary>
    /// ÉJÉÅÉâÇÃäpìxÇ©ÇÁà⁄ìÆÉxÉNÉgÉãÇåvéZ
    /// </summary>
    Vector3 CalcMovementFromCamera(Vector2 moveInput)
    {        
        var forward = transform.forward;
        // forward.y = 0;
        return (transform.right * moveInput.x) + (forward * moveInput.y);
    }

    /// <summary>
    /// äpìxÇ…êßå¿ÇÇ©ÇØÇÈ
    /// </summary>
    private void LimitAngle()
    {
        //0Å`360 -> -180 Å` 180 
        var rot_x = (transform.localRotation.eulerAngles.x > 180f) ?
            transform.localRotation.eulerAngles.x - 360 : transform.localRotation.eulerAngles.x;
        
        var rot_y = transform.localRotation.eulerAngles.y;
        rot_x = Mathf.Clamp(rot_x, -limitAngle, limitAngle);
        //-180 Å` 180 -> 0Å`360
        rot_x = (rot_x < 0) ?
            rot_x + 360 : rot_x;

        var rot = transform.localRotation.eulerAngles;
        rot.x = rot_x;
        transform.localRotation= Quaternion.Euler(rot);
    }
}
