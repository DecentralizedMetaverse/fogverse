using Cysharp.Threading.Tasks;
using DC;
using MistNet;
using StarterAssets;
using Teo.AutoReference;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// OnlinePlayerの初期設定
/// </summary>
public class PlayerSetup : MonoBehaviour
{
    [SerializeField] private DB_Player dbPlayer;
    [Get, SerializeField] private MistAnimator mistAnimator;
    [Get, SerializeField] private Animator animator;
    [Get, SerializeField] private MovementController thirdPersonController;
    [Get, SerializeField] private MistSyncObject syncObject;
    [Get,SerializeField] private PlayerInput input;

    private async void Start()
    {
        mistAnimator.Animator = animator;

        await UniTask.Delay(100);
        if (!syncObject.IsOwner)
        {
            thirdPersonController.enabled = false;
            return;
        }

        GM.Add<bool>("SetEnableMove", (value) => input.enabled = value);

        dbPlayer.user = transform;
        if (dbPlayer.worldRoot == null)
        {
            dbPlayer.worldRoot = new GameObject("root").transform;
        }
    }
}
