using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using DC;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Worldを構築する
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    [SerializeField] DB_Player db;

    Dictionary<string, Transform> worldContent = new(1024);
    Dictionary<string, Dictionary<string, object>> metaObjs = new(1024);

    private void Awake()
    {
        GM.Add<string, UniTask>("GenerateWorld", GenerateWorldAsync);
        GM.Add<string, UniTask<string>>("DownloadContent", DownloadContent);
        GM.Add<string, Transform, UniTask<Transform>>("GenerateObjectByCID", GenerateByCID);
    }

    /// <summary>
    /// 非同期でWorldの生成を行う
    /// </summary>
    /// <param name="cid"></param>
    async UniTask GenerateWorldAsync(string cid)
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
        var metaData = await GetMetaData(cid);

        // generate object
        var fileCID = metaData["cid"].ToString();

        Debug.Log($"[Debug][Content][0] GenerateByCID: {fileCID}");

        Transform transform;
        if (GM.Msg<bool>("IsBasicObject", fileCID))
        {
            Debug.Log($"[Content] BasicObject: {fileCID}");
            // 基本Object (Cube, Sphere, Plane, etc.)である場合
            // TODO: 確認
            transform = GM.Msg<Transform>("GenerateBasicObject", fileCID);
            transform.SetParent(parent);
        }
        else
        {
            Debug.Log($"[Content] Object: {fileCID}");
            // それ以外のObjectである場合
            transform = await GenerateObject(parent, metaData, fileCID);
        }

        Debug.Log($"[Debug][Content][1] GenerateByCID: {fileCID}");

        // download child object
        foreach (var obj in (List<object>)metaData["objs"])
        {
            // 再帰的にObjectを生成
            await GenerateByCID(obj.ToString(), transform);
        }

        // download components
        foreach (var componentCID in (List<object>)metaData["components"])
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
        var filePath = string.Format(Constants.MetaPath, $"{cid}.yaml");

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
    /// <param name="metaData"></param>
    /// <param name="fileCID"></param>
    /// <returns></returns>
    async UniTask<Transform> GenerateObject(Transform parent, Dictionary<string, object> metaData, string fileCID)
    {
        Debug.Log($"[Content] GenerateObject: {metaData["name"]}");
        Transform generateContent;
        var objType = metaData["type"].ToString();

        if (string.IsNullOrEmpty(fileCID))
        {
            Debug.Log($"[Content] GenerateObject: {metaData["name"]} is file empty");
            var obj = new GameObject();
            generateContent = obj.transform;
        }
        else
        {
            Debug.Log($"[Content] GenerateObject: {metaData["name"]} is file not empty");
            // ContentのmetaFileをDownloadし、Object生成
            generateContent = await GenerateContent(fileCID);
        }

        var objName = metaData["name"].ToString();

        var json = JsonConvert.SerializeObject(metaData["transform"]);
        var objTransform = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        SetTransform(generateContent.gameObject, parent, objName, objTransform);
        return generateContent;
    }

    /// <summary>
    /// ContentをDownloadし、World空間に配置する
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    async UniTask<Transform> GenerateContent(string cid)
    {
        // ContentをDownload
        var filePath = await DownloadContent(cid);

        Debug.Log($"[Content] Generate Object: {filePath}");

        // World空間に配置
        var generateContent = GM.Msg<Transform>("GenerateObj", filePath);

        return generateContent;
    }

    async UniTask GenerateComponent(Transform root, string metaCID)
    {
        Debug.Log($"[Content] GenerateComponent: {metaCID}");
        var metaData = await GetMetaData(metaCID);

        // ContentをDownload
        var fileCID = metaData["cid"].ToString();
        var fileName = metaData["name"].ToString();
        var filePath = string.Format(Constants.ContentPath, fileName);
        await GM.Msg<UniTask<bool>>("IPFSDownload", fileCID, filePath);

        var json = JsonConvert.SerializeObject(metaData["custom"]);
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
    /// ContentをDownloadする
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    private async UniTask<string> DownloadContent(string cid)
    {
        Debug.Log($"[Content] DownloadContent: {cid}");
        var metaData = await GetMetaData(cid);

        // Worldか単体データか判定する
        var isWorldData = metaData.ContainsKey("transform");

        if (isWorldData)
        {
            // Worldである場合
            GenerateWorldAsync(cid).Forget();
            return "";
        }

        // ContentDownload
        var fileCID = metaData["cid"].ToString();
        var fileName = metaData["name"].ToString();
        Debug.Log($"[Debug][Content] DownloadContent fileName: {fileName}");
        var filePath = string.Format(Constants.ContentPath, fileName);
        await GM.Msg<UniTask<bool>>("IPFSDownload", fileCID, filePath);

        GM.Msg("ShortMessage", $"Downloaded {fileName}");
        return filePath;
    }

    private async UniTask<Dictionary<string, object>> GetMetaData(string cid)
    {
        Debug.Log($"[Content] GetMetaData: {cid}");
        if (metaObjs.TryGetValue(cid, out var metaData))
        {
            return metaData;
        }

        // metaFileDownload        
        var metaPath = await DownloadMetaFile(cid);

        // metaFile読み込み
        var data = GM.Msg<Dictionary<string, object>>("ReadYaml", metaPath);

        metaObjs.Add(cid, data);

        return data;
    }
}
