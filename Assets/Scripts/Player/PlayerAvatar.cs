using Cysharp.Threading.Tasks;
using MistNet;
using TC;
using Teo.AutoReference;
using UnityEngine;

public class PlayerAvatar : MonoBehaviour
{
    // private const string ShaderName = "VRM10/Universal Render Pipeline/MToon10";
    private const string ShaderName = "Universal Render Pipeline/Unlit";
    [Get, SerializeField] private MistSyncObject syncObject;
    [SerializeField] private RuntimeAnimatorController runtimeAnimator;
    [Get, SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform target;

    private Shader urpShader;

    private void Start()
    {
        urpShader = Shader.Find(ShaderName);
        if (!syncObject.IsOwner) return;
        Message.Subscribe<GameObject>("ChangeAvatar", ChangeAvatar);
    }

    private void ChangeAvatar(GameObject avatarObj)
    {
        if (target.childCount != 0)
        {
            target.DestroyChildren();
        }

        if (target == null)
        {
            Debug.LogError("player transform is null");
            return;
        }

        Debug.Log("Change Avatar");

        avatarObj.transform.SetParent(target);
        avatarObj.transform.localPosition = Vector3.zero;
        avatarObj.transform.localRotation = Quaternion.identity;
        avatarObj.transform.localScale = Vector3.one;

        ChangeMaterialShader(avatarObj);
        SetHeight(avatarObj);
        SetAnimator(avatarObj).Forget();
    }

    private async UniTask SetAnimator(GameObject obj)
    {
        if (!obj.TryGetComponent(out Animator avatarAnimator))
        {
            Debug.LogError("Animator not found");
            return;
        }

        // anim.runtimeAnimatorController = runtimeAnimator;
        // playerAnimator = anim;

        Debug.Log("[PlayerAvatar] SetAnimator");
        playerAnimator.avatar = avatarAnimator.avatar;
        avatarAnimator.runtimeAnimatorController = runtimeAnimator;

        playerAnimator.enabled = false;
        await UniTask.Yield();
        playerAnimator.Rebind();
        playerAnimator.Update(0f);
        playerAnimator.enabled = true;
        avatarAnimator.enabled = false;

        Debug.Log($"playerAnimator isHuman {playerAnimator.isHuman}");
        Debug.Log($"playerAnimator avatar {playerAnimator.avatar}");
        Debug.Log($"avatarAnimator isHuman {avatarAnimator.avatar.isHuman}");
        Debug.Log($"avatarAnimator avatar {avatarAnimator.avatar}");
    }

    private void ChangeMaterialShader(GameObject obj)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                material.shader = urpShader;
            }
        }
    }

    private void SetHeight(GameObject obj)
    {
        var anim = obj.GetComponent<Animator>();
        var head = anim.GetBoneTransform(HumanBodyBones.Head);
        cameraTransform.position = head.position;
    }
}
