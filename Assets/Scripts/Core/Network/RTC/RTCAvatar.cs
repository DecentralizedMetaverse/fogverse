using Cysharp.Threading.Tasks;
using DC;
using UnityEngine;

public class RTCAvatar : MonoBehaviour
{
    private void Start()
    {
        GM.Msg("AddAvatar", this);
    }

    public async void SetAvatar(string cid)
    {
        var avatarGameObject = await GM.Msg<UniTask<GameObject>>("DownloadAvatar");
        avatarGameObject.transform.SetParent(transform);
    }
}
