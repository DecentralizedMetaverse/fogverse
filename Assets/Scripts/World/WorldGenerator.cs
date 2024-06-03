using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DC;
using UnityEngine;
using System.Security.Cryptography;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Worldを構築する
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    [SerializeField] DB_Player db;
    string path = "";

    Dictionary<string, Transform> worldContent = new(1024);
    Dictionary<string, Dictionary<string, object>> metaObjs = new(1024);

    private void Awake()
    {
        path = $"{Application.dataPath}/{GM.mng.metaPath}";
        GM.Add<string>("GenerateWorld", GenerateWorldAsync);
        GM.Add<string, UniTask<string>>("DownloadContent", DownloadContent);
        GM.Add<string, Transform, UniTask<Transform>>("GenerateObjectByCID", GenerateByCID);
    }

    /// <summary>
    /// 非同期でWorldの生成を行う
    /// </summary>
    /// <param name="cid"></param>
    async void GenerateWorldAsync(string cid)
    {
        if (db.worldRoot != null)
        {
            // db.worldRoot.DestroyChildren();
            Destroy(db.worldRoot.gameObject);
        }
        db.worldRoot = await GenerateByCID(cid, null);
        GM.Msg("UpdateWorldID", cid);
    }

    /// <summary>
    /// 再帰的にObjectを生成する
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    async UniTask<Transform> GenerateByCID(string cid, Transform parent)
    {
        Debug.Log($"[Content] GenerateByCID: {cid}");
        // read meta file
        var data = await GetMetaData(cid);

        // generate object
        var fileCID = data["cid"].ToString();
        Transform transform = null;
        if (GM.Msg<bool>("IsBasicObject", fileCID))
        {
            // 基本Object (Cube, Sphere, Plane, etc.)である場合
            // TODO: 確認
            transform = GM.Msg<Transform>("GenerateBasicObject", fileCID);
            transform.SetParent(parent);
        }
        else
        {
            transform = await GenerateObject(parent, data, fileCID);
        }

        // download child object
        foreach (var obj in (List<object>)data["objs"])
        {
            // 再帰的にObjectを生成
            await GenerateByCID(obj.ToString(), transform);
        }

        // download components
        foreach (var componentCID in (List<object>)data["components"])
        {
            await GenerateComponent(transform, componentCID.ToString());
        }

        return transform;
    }

    /// <summary>
    /// MetaFileをDownloadする
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    private async UniTask<string> DownloadMetaFile(string cid)
    {
        var filePath = $"{path}/{cid}.yaml";

        if (!File.Exists(filePath))
        {
            await GM.Msg<UniTask<bool>>("IPFSDownload", cid, filePath);
        }

        return filePath;
    }

    /// <summary>
    /// Object生成
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="data"></param>
    /// <param name="fileCID"></param>
    /// <returns></returns>
    async UniTask<Transform> GenerateObject(Transform parent, Dictionary<string, object> data, string fileCID)
    {
        Debug.Log($"[Content] GenerateObject: {data["name"]}");
        Transform transform;
        if (fileCID != "")
        {
            // ContentのmetaFileをDownloadしObject生成
            transform = await GenerateContent(fileCID);
        }
        else
        {
            GameObject obj = new GameObject();
            transform = obj.transform;
        }

        var objName = data["name"].ToString();
        var json = JsonConvert.SerializeObject(data["transform"]);
        var objTransform = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        SetTransform(transform.gameObject, parent, objName, objTransform);
        return transform;
    }

    async UniTask GenerateComponent(Transform root, string metaCID)
    {
        Debug.Log($"[Content] GenerateComponent: {metaCID}");
        var data = await GetMetaData(metaCID);

        // ContentをDownload
        var fileCID = data["cid"].ToString();
        var fileName = data["name"].ToString();
        var filePath = $"{Application.dataPath}/{GM.mng.contentPath}/{fileName}";
        await GM.Msg<UniTask<bool>>("IPFSDownload", fileCID, filePath);

        var json = JsonConvert.SerializeObject(data["custom"]);
        var custom = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        GM.Msg("GenerateComponent", filePath, root, custom);
    }

    /// <summary>
    /// 座標等を設定
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="parent"></param>
    /// <param name="objName"></param>
    /// <param name="objTransform"></param>
    private void SetTransform(GameObject obj, Transform parent, string objName, Dictionary<string, string> objTransform)
    {
        obj.name = objName;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = objTransform["position"].ToVector3();
        obj.transform.localRotation = Quaternion.Euler(objTransform["rotation"].ToVector3());
        obj.transform.localScale = objTransform["scale"].ToVector3();
    }

    /// <summary>
    /// ContentをDownloadし、World空間に配置する
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    async UniTask<Transform> GenerateContent(string cid)
    {
        // ContentをDownload
        string filePath = await DownloadContent(cid);

        // World空間に配置
        var transform = GM.Msg<Transform>("GenerateObj", filePath);
        return transform;
    }

    /// <summary>
    /// ContentをDownloadする
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    private async UniTask<string> DownloadContent(string cid)
    {
        Debug.Log($"[Content] DownloadContent: {cid}");
        Dictionary<string, object> data = await GetMetaData(cid);

        // MetaFileの種別判定
        if (data.ContainsKey("transform"))
        {
            // Worldである場合
            GenerateWorldAsync(cid);
            return "";
        }

        // ContentDownload
        var fileCID = data["cid"].ToString();
        var fileName = data["name"].ToString();
        var filePath = $"{Application.dataPath}/{GM.mng.contentPath}/{fileName}";
        await GM.Msg<UniTask<bool>>("IPFSDownload", fileCID, filePath);

        GM.Msg("ShortMessage", $"Downloaded {fileName}");
        return filePath;
    }

    private async UniTask<Dictionary<string, object>> GetMetaData(string cid)
    {
        if (metaObjs.ContainsKey(cid))
        {
            return metaObjs[cid];
        }

        // metaFileDownload        
        var metaPath = await DownloadMetaFile(cid);

        // metaFile読み込み
        var data = GM.Msg<Dictionary<string, object>>("ReadYaml", metaPath);

        metaObjs.Add(cid, data);

        return data;
    }
}
