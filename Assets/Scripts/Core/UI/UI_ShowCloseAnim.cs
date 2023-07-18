using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UI_ShowCloseAnim : UI_ShowClose
{
    Animator anim;
    readonly int animActive = Animator.StringToHash("active");

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public override void Show()
    {
        StartShow();

        anim.SetBool(animActive, true);

        EndShow();
    }

    public override void Close()
    {
        StartClose();

        anim.SetBool(animActive, true);

        EndClose();
    }
}