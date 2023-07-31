using Cinemachine;
using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] float distanceMin = 1f;
    [SerializeField] float distanceMax = 5f;

    float scrollDivide = 1.0f / 360;
    private Cinemachine3rdPersonFollow thirdperson;

    void Start()
    {
        InputF.action.Game.Scroll.performed += OnScroll;
        thirdperson = cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    /// <summary>
    /// CameraÇÃãóó£ÇïœçXÇ∑ÇÈ
    /// </summary>
    /// <param name="context"></param>
    private void OnScroll(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<float>();
        var distance = thirdperson.CameraDistance;
        distance -= value * scrollDivide;
        thirdperson.CameraDistance = Mathf.Clamp(distance, distanceMin, distanceMax);
    }
}
