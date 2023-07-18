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
/// World���\�z����
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
    /// �񓯊���World�̐������s��
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
    /// �ċA�I��Object�𐶐�����
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    async UniTask<Transform> GenerateByCID(string cid, Transform parent)
    {
        // read meta file
        var data = await GetMetaData(cid);

        // generate object
        var fileCID = data["cid"].ToString();
        Transform transform = null;
        if (GM.Msg<bool>("IsBasicObject", fileCID))
        {
            // ��{Object (Cube, Sphere, Plane, etc.)�ł���ꍇ
            // TODO: �m�F
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
            // �ċA�I��Object�𐶐�
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
    /// MetaFile��Download����
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
    /// Object����
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="data"></param>
    /// <param name="fileCID"></param>
    /// <returns></returns>
    async UniTask<Transform> GenerateObject(Transform parent, Dictionary<string, object> data, string fileCID)
    {
        Transform transform;
        if (fileCID != "")
        {
            // Content��metaFile��Download��Object����
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
        var data = await GetMetaData(metaCID);

        // Content��Download
        var fileCID = data["cid"].ToString();
        var fileName = data["name"].ToString();
        var filePath = $"{Application.dataPath}/{GM.mng.contentPath}/{fileName}";
        await GM.Msg<UniTask<bool>>("IPFSDownload", fileCID, filePath);

        var json = JsonConvert.SerializeObject(data["custom"]);
        var custom = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        GM.Msg("GenerateComponent", filePath, root, custom);
    }

    /// <summary>
    /// ���W����ݒ�
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
    /// Content��Download���AWorld��Ԃɔz�u����
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    async UniTask<Transform> GenerateContent(string cid)
    {
        // Content��Download
        string filePath = await DownloadContent(cid);

        // World��Ԃɔz�u
        var transform = GM.Msg<Transform>("GenerateObj", filePath);
        return transform;
    }

    /// <summary>
    /// Content��Download����
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    private async UniTask<string> DownloadContent(string cid)
    {
        Dictionary<string, object> data = await GetMetaData(cid);

        // MetaFile�̎�ʔ���
        if (data.ContainsKey("transform"))
        {
            // World�ł���ꍇ
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

        // metaFile�ǂݍ���
        var data = GM.Msg<Dictionary<string, object>>("ReadYaml", metaPath);

        metaObjs.Add(cid, data);

        return data;
    }
}
