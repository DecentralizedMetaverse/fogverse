using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform tra;
    private void Awake()
    {
        tra = Camera.main.transform;
    }
    private void LateUpdate()
    {
        transform.rotation = tra.rotation;
    }
}
