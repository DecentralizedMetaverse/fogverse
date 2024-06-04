using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EventObject3D : EventObject
{
    private void Awake()
    {
        Rigidbody rb;
        if (TryGetComponent(out rb))
        {
            rb.useGravity = false;
        }
    }
}
