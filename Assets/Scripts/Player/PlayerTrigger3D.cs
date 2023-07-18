using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PlayerTrigger3D : PlayerTrigger
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Event") return;

        AddRunnableObj(other.gameObject);
        GM.Msg("Hint", exeEvents[0].GetHint(), true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag != "Event") return;

        RemoveRunnableObj(other.gameObject);
        ShowHintHandler();
    }
}
