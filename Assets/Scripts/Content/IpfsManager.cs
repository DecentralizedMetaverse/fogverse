using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using DC;
using UnityEngine;
using System.IO;

/// <summary>
/// IPFS�Ƃ������s���v���O����
/// </summary>
public class IpfsManager : MonoBehaviour
{
    void Start()
    {
        GM.Add<string, UniTask<string>>("IPFSUpload", Upload);
        GM.Add<string, string, UniTask<bool>>("IPFSDownload", Download);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>true: ����</returns>
    async UniTask<string> Upload(string filePath)
    {
        //var path = GetPath(fileName);
        if (!GM.Msg<bool>("EncryptFile", filePath)) return "";
        var ret = await GM.Msg<UniTask<string>>("Exe", "ipfs", $"add \"{filePath}.enc\"");
        File.Delete($"{filePath}.enc"); // File�폜

        GM.Log($"{ret}");
        var words = ret.Split(' ');
        if (words.Length <= 1) return "";
        
        return words[1];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="filePath"></param>
    /// <returns>true: ����</returns>
    async UniTask<bool> Download(string cid, string filePath)
    {
        var ret = await GM.Msg<UniTask<string>>("Exe", "ipfs", $"get {cid} -o \"{filePath}.enc\"");
        
        if (!GM.Msg<bool>("DecryptFile", $"{filePath}.enc")) return false;
        File.Delete($"{filePath}.enc"); // File�폜

        return true;
    }   
}
