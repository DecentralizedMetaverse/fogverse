using UnityEngine;
using DC;
using System.IO;
using Cysharp.Threading.Tasks;

public class AvatarModel : MonoBehaviour
{
    const string avatarPassword = "01MZwoqRmvuGK!FP$a";

    void Start()
    {        
        GM.Add<string, UniTask<string>>("UploadAvatar", UploadAvatar);
        GM.Add<string, UniTask<GameObject>>("DownloadAvatar", DownloadAvatar);
        GM.Add<string, GameObject>("LoadAvatar", LoadAvatar);
    }       

    /// <summary>
    /// Avatar��IPFS��Upload����
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    async UniTask<string> UploadAvatar(string path)
    {
        var fileName = Path.GetFileName(path);
        var avatarPath = string.Format(Constants.AvatarPath, fileName);
        
        // Copy
        File.Copy(path, avatarPath, true);

        // Encrypt Avatar
        if (!GM.Msg<bool>("EncryptFileWithPassword", avatarPath, avatarPassword))
        {
            Debug.LogError("[Error] Avatar Uploading");
            return "";
        }
        
        var cid = await GM.Msg<UniTask<string>>("IPFSUpload", $"{avatarPath}.enc");
        return cid;
    }

    /// <summary>
    /// IPFS����Download���s��
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    async UniTask<GameObject> DownloadAvatar(string cid)
    {
        var encryptAvatarPath = string.Format(Constants.AvatarPath, $"{cid}.enc");

        // Download from IPFS
        var result = await GM.Msg<UniTask<bool>>("IPFSDownload", cid, encryptAvatarPath);
        if(!result)
        {
            Debug.LogError("[Error] Avatar Downloading");
            return null;
        }

        // Decrypt Avatar
        var data = await GM.Msg<UniTask<byte[]>>("GetDecryptDataWithPassword", encryptAvatarPath, avatarPassword);
        var avatar = GM.Msg<GameObject>("VRMModelLoadFromData", data);

        return avatar;
    }

    /// <summary>
    /// Load from local storage 
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    GameObject LoadAvatar(string path)
    {
        var avatar = GM.Msg<GameObject>("VRMModelLoad", path);

        return avatar;
    }
}
