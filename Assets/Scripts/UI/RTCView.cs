using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTCView : MonoBehaviour
{
    [SerializeField] UI_ToggleFade toggle;

    void Start()
    {
        GM.Add("ShowRTC", Show);
    }

    void Show()
    {
        toggle.active = !toggle.active ? true : false;
    }
}
