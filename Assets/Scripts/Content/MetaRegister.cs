using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// 配置されたObjectのMetaFileを作成する
/// </summary>
public class MetaRegister : MonoBehaviour
{
    [SerializeField] DB_Player db;

    void Start()
    {
        GM.Add("RegisterObject", RegisterObject);
    }

    async void RegisterObject()
    {
        var cid = await UpdateMeta(db.worldRoot);
        GM.Msg("UpdateWorldID", cid);
    }
    
    /// <summary>
    /// MetaFileを作成する
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    async UniTask<string> UpdateMeta(Transform transform)
    {
        List<string> objs = new();

        // does this have a file?
        var fileName = "";
        
        if (transform.TryGetComponent(out ObjectBase objBase))
        {
            fileName = objBase.fileName;
        }
        else
        {
            // 子階層にアクセス
            foreach (Transform child in transform)
            {
                var childMetaCID = await UpdateMeta(child);
                objs.Add(childMetaCID);
            }
        }

        // MetaFileの作成
        var metaCID = await CreateYamlObjectData(fileName, transform, objs);
        
        return metaCID;
    }

    /// <summary>
    /// YamlにObjectの内容を書き出す
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    async UniTask<string> CreateYamlObjectData(string fileName, Transform transform, List<string> objs)
    {
        // Upload the file to IPFS
        var cid = "";
        if (fileName != "")
        {
            if (GM.Msg<bool>("IsBasicObject", fileName))
            {
                // TODO: 確認
                cid = fileName;
            }
            else
            {
                cid = await CreateYamlAndUploadContent(fileName);
            }
        }

        Dictionary<string, object> data = new();
        data.Add("name", transform.gameObject.name);
        data.Add("cid", cid);

        // Transform
        Dictionary<string, string> location = new();
        location.Add("position", transform.position.ToSplitString());
        location.Add("rotation", transform.rotation.eulerAngles.ToSplitString());
        location.Add("scale", transform.localScale.ToSplitString());
        data.Add("transform", location);
        
        // Child Objects
        data.Add("objs", objs);

        // Components
        List<string> comps = new(64);
        foreach (var comp in transform.GetComponents<Component>())
        {
            var componentCID = await CreateYamlComponentData(comp.fileName, comp.custom);
            comps.Add(componentCID);
        }
        data.Add("components", comps);


        // Fileの書き出し
        var metaCID = await WriteMeta(data);

        return metaCID;
    }

    /// <summary>
    /// Contentをアップロードし、YamlにContentの内容を書き出す
    /// </summary>
    /// <param name="path"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    async UniTask<string> CreateYamlAndUploadContent(string path)
    {
        // IPFSに登録
        var fileCID = await GM.Msg<UniTask<string>>("IPFSUpload", path);
        var fileName = System.IO.Path.GetFileName(path);
        
        Dictionary<string, object> data = new();
        data.Add("name", fileName);
        data.Add("cid", fileCID);

        // MetaFileの書き出し
        var metaCID = await WriteMeta(data);

        return metaCID;
    }

    /// <summary>
    /// MetaFile書き込み
    /// </summary>
    /// <param name="yamlData"></param>
    /// <returns>保存したFileパス</returns>
    async UniTask<string> WriteMeta(Dictionary<string, object> yamlData)
    {
        // 仮のFileを作成
        Guid guid = Guid.NewGuid();
        string guidString = guid.ToString();

        var path = $"{Application.dataPath}/{GM.mng.metaPath}/{guidString}.yaml";
        GM.Msg("WriteYaml", path, yamlData);

        // IPFSへ登録
        var cid = await GM.Msg<UniTask<string>>("IPFSUpload", path);

        // TMPFileの削除
        File.Delete(path);

        // 書き出し
        var outputPath = $"{Application.dataPath}/{GM.mng.metaPath}/{cid}.yaml";
        GM.Msg("WriteYaml", outputPath, yamlData);

        return System.IO.Path.GetFileNameWithoutExtension(outputPath);
    }

    /// <summary>
    /// ComponentのMetaFileを作成し、IPFSに登録する
    /// </summary>
    /// <param name="path"></param>
    /// <param name="custom"></param>
    /// <returns></returns>
    async UniTask<string> CreateYamlComponentData(string path, Dictionary<string, object> custom)
    {
        // IPFSに登録
        var fileCID = await GM.Msg<UniTask<string>>("IPFSUpload", path);
        var fileName = System.IO.Path.GetFileName(path);
        Dictionary<string, object> data = new();
        data.Add("name", fileName);
        data.Add("cid", fileCID);
        data.Add("custom", custom);

        // MetaFileの書き出し
        var metaCID = await WriteMeta(data);

        return metaCID;
    }
}
