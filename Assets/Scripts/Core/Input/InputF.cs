using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputF : MonoBehaviour
{
    public static @InputActions action;

    private void Awake()
    {
        action = new @InputActions();
        action.Enable();
    }
}
