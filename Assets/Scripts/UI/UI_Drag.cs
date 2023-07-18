using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UI_Drag : MonoBehaviour, IDragHandler
{
    Vector3 pos;

    void Start()
    {
        InputF.action.Game.Submit.canceled += OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext obj)
    {
        pos = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (pos != Vector3.zero)
        {
            transform.position += Input.mousePosition - pos;
        }
        pos = Input.mousePosition;
    }
}
