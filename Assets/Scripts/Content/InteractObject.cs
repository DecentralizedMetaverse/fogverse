using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// ObjectÇëÄçÏÇ∑ÇÈ
/// </summary>
public class InteractObject : MonoBehaviour
{
    private RaycastHit hit;

    void Start()
    {
        InputF.action.Game.Submit.performed += OnSubmit;
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (GM.pause != ePause.mode.GameStop) return;

        if (!InputF.action.Game.Dash.IsPressed()) return;

        var pos = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(pos, out hit, 5000))
        {
            var root = hit.transform;
            GM.Msg("ShowObjectMenu", root);
        }
    }
}
