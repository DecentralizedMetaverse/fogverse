using System;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChatController : MonoBehaviour
{
    void Start()
    {        
        InputF.action.Game.Chat.performed += OnChat;
    }

    private void OnCancel(InputAction.CallbackContext obj)
    {
        GM.pause = ePause.mode.none;
        GM.Msg("CloseChat");
        InputF.action.UI.Cancel.performed -= OnCancel;
    }

    private void OnChat(InputAction.CallbackContext obj)
    {
        InputF.action.UI.Cancel.performed += OnCancel;
        GM.pause = ePause.mode.GameStop;
        GM.Msg("ShowChat");
    }

    
}
