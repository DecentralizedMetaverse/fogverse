using Cysharp.Threading.Tasks;
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
    /// <param name="targetTransform"></param>
    /// <returns></returns>
    async UniTask<string> CreateYamlObjectData(string fileName, Transform targetTransform, List<string> objs)
    {
        // Upload the file to IPFS
        var cid = "";
        if (fileName != "")
        {
            // BasicObjectは基本図形 (Cube, Sphere, etc.)
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

        var objType = "";
        if (targetTransform.TryGetComponent(out ObjectBase objInfo))
        {
            // World読み込みの際にTextやImageやAudioなどを適用するために使用
            objType = objInfo.ObjType;
        }

        Dictionary<string, object> metaData = new()
        {
            { "name", targetTransform.gameObject.name },
            { "cid", cid },
            { "type", objType }
        };

        // Transform
        Dictionary<string, string> location = new()
        {
            { "position", targetTransform.position.ToSplitString() },
            { "rotation", targetTransform.rotation.eulerAngles.ToSplitString() },
            { "scale", targetTransform.localScale.ToSplitString() }
        };
        metaData.Add("transform", location);

        // Child Objects
        metaData.Add("objs", objs);

        // Components
        List<string> comps = new(64);
        foreach (var comp in targetTransform.GetComponents<Component>())
        {
            // TODO: Componentの保存方法を検討しなおす
            var componentCID = await CreateYamlComponentData(comp.fileName, comp.custom);
            comps.Add(componentCID);
        }
        metaData.Add("components", comps);

        // Fileの書き出し
        var metaCID = await WriteMeta(metaData);

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
        var fileName = Path.GetFileName(path);

        Debug.Log($"[Content] Upload: {fileName} {fileCID}");
        
        Dictionary<string, object> data = new()
        {
            { "name", fileName },
            { "cid", fileCID }
        };

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
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        var metaPath = string.Format(Constants.MetaPath, $"{guidString}.yaml");
        GM.Msg("WriteYaml", metaPath, yamlData);

        // IPFSへ登録
        var cid = await GM.Msg<UniTask<string>>("IPFSUpload", metaPath);

        // TMPFileの削除
        File.Delete(metaPath);

        // 書き出し
        var outputPath = string.Format(Constants.MetaPath, $"{cid}.yaml");
        GM.Msg("WriteYaml", outputPath, yamlData);

        return Path.GetFileNameWithoutExtension(outputPath);
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
        var fileName = Path.GetFileName(path);
        Dictionary<string, object> data = new();
        data.Add("name", fileName);
        data.Add("cid", fileCID);
        data.Add("custom", custom);

        // MetaFileの書き出し
        var metaCID = await WriteMeta(data);

        return metaCID;
    }
}
