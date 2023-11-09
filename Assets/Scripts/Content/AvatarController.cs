using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DC;
using UnityEngine;

[Obsolete]
public class AvatarController : MonoBehaviour
{
    /// <summary>
    /// CID, Avatar
    /// </summary>
    Dictionary<string, GameObject> avatars = new();

    async void Start()
    {
        // ����GameObject�IAvatar��v������
        var avatarCID = GM.Msg<object>("GetSaveData", "avatar");
        if (avatarCID == null) return;
        var avatarObj = await GM.Msg<UniTask<GameObject>>("DownloadAvatar", avatarCID.ToString());

    }
}
