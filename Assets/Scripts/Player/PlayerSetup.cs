using Cysharp.Threading.Tasks;
using MistNet;
using StarterAssets;
using Teo.AutoReference;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [SerializeField] private DB_Player dbPlayer;
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
            return;
        }

        dbPlayer.user = transform;
        if (dbPlayer.worldRoot == null)
        {
            dbPlayer.worldRoot = new GameObject("root").transform;
        }
    }
}
