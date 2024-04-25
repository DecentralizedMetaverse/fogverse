using Cysharp.Threading.Tasks;
using MistNet;
using StarterAssets;
using Teo.AutoReference;
using UnityEngine;

public class PlayerAvatarSetup : MonoBehaviour
{
    [Get, SerializeField] private MistAnimator mistAnimator;
    [Get, SerializeField] private Animator animator;
    [Get, SerializeField] private ThirdPersonController thirdPersonController;
    [Get, SerializeField] private MistSyncObject syncObject;

    private async void Start()
    {
        mistAnimator.Animator = animator;

        await UniTask.Delay(100);
        if (!syncObject.IsOwner)
        {
            thirdPersonController.enabled = false;
        }
    }
}
