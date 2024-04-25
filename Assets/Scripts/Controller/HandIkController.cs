using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandIkController : MonoBehaviour
{
    [SerializeField] Animator animator;
    public bool ikActive = false;
    public Transform rightHandObj = null;
    public Transform lookObj = null;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK()
    {
        if (!animator) return;

        if (ikActive)
        {
            SetLookAtWeight();
            SetRightHandIK();
        }
        else
        {
            ResetIK();
        }
    }

    private void SetLookAtWeight()
    {
        if (lookObj != null)
        {
            animator.SetLookAtWeight(1);
            animator.SetLookAtPosition(lookObj.position);
        }
    }

    private void SetRightHandIK()
    {
        if (rightHandObj != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
        }
    }

    private void ResetIK()
    {
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        animator.SetLookAtWeight(0);
    }
}
