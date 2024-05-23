using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Objectを操作する
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
        if (InputController.I.Mode != InputMode.UIOnly) return;

        if (!InputF.action.Game.Sprint.IsPressed()) return;

        var pos = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(pos, out hit, 5000))
        {
            var root = hit.transform;
            GM.Msg("ShowObjectMenu", root);
        }
    }
}
