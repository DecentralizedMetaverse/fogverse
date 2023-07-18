using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Transform triggerCenter;
    
    const float rotationSpeed = 0.5f;

    public void SetMove(Vector3 move)
    {
        var speed = move == Vector3.zero ? 0f : 1f;
        var run = move == Vector3.zero ? false : true;
        anim.SetFloat("speed", speed);
        anim.SetBool("run", run);

        if (speed <= 0f) return;

        SetAngle(move);
        anim.SetFloat("x", move.x);
        anim.SetFloat("z", move.z);
    }

    void SetAngle(Vector3 move)
    {
        var deg = move.ToDeg();
        var rot = triggerCenter.rotation.eulerAngles;
        rot.y = -deg + 90f;
        triggerCenter.rotation = triggerCenter.rotation.Lerp(rot.ToRotation(), rotationSpeed);
    }
}
