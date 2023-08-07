using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputF : MonoBehaviour
{
    public static @InputActions action;

    private void Awake()
    {
        action = new @InputActions();
        action.Enable();
    }

    public static void SetOperation(ePause.mode mode)
    {
        switch (mode)
        {
            case ePause.mode.none:
                // GM.Msg("SetOperation", "Game");
                // action.Edit.Enable();
                action.Game.Enable();
                action.UI.Disable();
                break;
            case ePause.mode.GameStop:
                // GM.Msg("SetOperation", "UI");
                action.UI.Enable();
                action.Game.Disable();
                break;
            case ePause.mode.UIStop:
                // GM.Msg("SetOperation", "Game");
                action.Game.Enable();
                action.UI.Disable();
                break;
        }
    }
}
