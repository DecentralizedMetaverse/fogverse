using MistNet;
using TC;
using Teo.AutoReference;
using UnityEngine;

public class PlayerAvatar : MonoBehaviour
{
    private const string ShaderName = "VRM10/Universal Render Pipeline/MToon10";
    // private const string ShaderName = "Universal Render Pipeline/Unlit";
    [Get, SerializeField] private MistSyncObject syncObject;
    [Get, SerializeField] private MistAnimator mistAnimator;
    [SerializeField] private RuntimeAnimatorController animator;

    [SerializeField] private Transform target;

    private Shader _urpShader;

    private void Start()
    {
        _urpShader = Shader.Find(ShaderName);
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

        if (!avatarObj.TryGetComponent(out Animator anim))
        {
            Debug.LogError("Animator not found");
            return;
        }

        anim.runtimeAnimatorController = animator;
    }
}
