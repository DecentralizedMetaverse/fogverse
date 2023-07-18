using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// �z�u���ꂽObject��MetaFile���쐬����
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
    /// MetaFile���쐬����
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
            // �q�K�w�ɃA�N�Z�X
            foreach (Transform child in transform)
            {
                var childMetaCID = await UpdateMeta(child);
                objs.Add(childMetaCID);
            }
        }

        // MetaFile�̍쐬
        var metaCID = await CreateYamlObjectData(fileName, transform, objs);
        
        return metaCID;
    }

    /// <summary>
    /// Yaml��Object�̓��e�������o��
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
                // TODO: �m�F
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


        // File�̏����o��
        var metaCID = await WriteMeta(data);

        return metaCID;
    }

    /// <summary>
    /// Content���A�b�v���[�h���AYaml��Content�̓��e�������o��
    /// </summary>
    /// <param name="path"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    async UniTask<string> CreateYamlAndUploadContent(string path)
    {
        // IPFS�ɓo�^
        var fileCID = await GM.Msg<UniTask<string>>("IPFSUpload", path);
        var fileName = System.IO.Path.GetFileName(path);
        
        Dictionary<string, object> data = new();
        data.Add("name", fileName);
        data.Add("cid", fileCID);

        // MetaFile�̏����o��
        var metaCID = await WriteMeta(data);

        return metaCID;
    }

    /// <summary>
    /// MetaFile��������
    /// </summary>
    /// <param name="yamlData"></param>
    /// <returns>�ۑ�����File�p�X</returns>
    async UniTask<string> WriteMeta(Dictionary<string, object> yamlData)
    {
        // ����File���쐬
        Guid guid = Guid.NewGuid();
        string guidString = guid.ToString();

        var path = $"{Application.dataPath}/{GM.mng.metaPath}/{guidString}.yaml";
        GM.Msg("WriteYaml", path, yamlData);

        // IPFS�֓o�^
        var cid = await GM.Msg<UniTask<string>>("IPFSUpload", path);

        // TMPFile�̍폜
        File.Delete(path);

        // �����o��
        var outputPath = $"{Application.dataPath}/{GM.mng.metaPath}/{cid}.yaml";
        GM.Msg("WriteYaml", outputPath, yamlData);

        return System.IO.Path.GetFileNameWithoutExtension(outputPath);
    }

    /// <summary>
    /// Component��MetaFile���쐬���AIPFS�ɓo�^����
    /// </summary>
    /// <param name="path"></param>
    /// <param name="custom"></param>
    /// <returns></returns>
    async UniTask<string> CreateYamlComponentData(string path, Dictionary<string, object> custom)
    {
        // IPFS�ɓo�^
        var fileCID = await GM.Msg<UniTask<string>>("IPFSUpload", path);
        var fileName = System.IO.Path.GetFileName(path);
        Dictionary<string, object> data = new();
        data.Add("name", fileName);
        data.Add("cid", fileCID);
        data.Add("custom", custom);

        // MetaFile�̏����o��
        var metaCID = await WriteMeta(data);

        return metaCID;
    }
}
