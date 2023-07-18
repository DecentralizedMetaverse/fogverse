using System;
using System.Collections;
using System.Collections.Generic;
using DC.Player;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BaseController))]

public class OfflineController : MonoBehaviour
{
    BaseController controller;
    Vector2 moveVector;
    private bool dash;

    void Start()
    {
        controller = GetComponent<BaseController>();
        controller.Init();
        
        InputF.action.Game.Move.started += OnMove;
        InputF.action.Game.Move.performed += OnMove;
        InputF.action.Game.Move.canceled += OnMove;
        InputF.action.Game.Jump.performed += OnJump;
        InputF.action.Game.Dash.started += OnStartDash;
        InputF.action.Game.Dash.canceled += OnEndDash;
    }


    void FixedUpdate()
    {
        controller.Move(moveVector, dash);
    }

    private void OnMove(InputAction.CallbackContext contex)
    {
        moveVector = contex.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext contex)
    {
        controller.Jump();
    }    
    
    private void OnStartDash(InputAction.CallbackContext obj)
    {
        dash = true;
    }
    
    private void OnEndDash(InputAction.CallbackContext obj)
    {
        dash = false;
    }
}
