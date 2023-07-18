using Cysharp.Threading.Tasks;
using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarController : MonoBehaviour
{
    /// <summary>
    /// CID, Avatar
    /// </summary>
    Dictionary<string, GameObject> avatars = new();
    
    async void Start()
    {
        var avatarCID = GM.Msg<object>("GetSaveData", "avatar");
        if (avatarCID == null) return;
        var avatarObj = await GM.Msg<UniTask<GameObject>>("DownloadAvatar", avatarCID.ToString());

    }       
}
