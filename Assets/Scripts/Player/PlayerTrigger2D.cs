using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class PlayerTrigger2D : PlayerTrigger
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Event") return;

        AddRunnableObj(other.gameObject);        
        GM.Msg("Hint", exeEvents[0].GetHint(), true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag != "Event") return;

        RemoveRunnableObj(other.gameObject);
        ShowHintHandler();
    }
}
