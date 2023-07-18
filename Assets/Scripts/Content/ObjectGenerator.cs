
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DC;
using UnityEngine;
using UniGLTF;
using VRM;
using DG.Tweening.Plugins.Core.PathCore;

/// <summary>
/// WorldãÛä‘Ç…ObjectÇê∂ê¨Ç∑ÇÈ
/// </summary>
public class ObjectGenerator : MonoBehaviour
{
    [SerializeField] DB_Player db;
    [SerializeField] ObjectText prefabText;
    [SerializeField] VideoObject prefabVideo;
    [SerializeField] GameObject prefabUnknown;
    [SerializeField] ObjectSSH prefabSSH;

    Dictionary<string, Delegate> functions = new(64);


    void Start()
    {
        GM.Add<string, Transform>("GenerateObj", Generate);
        AddFunc<string, Transform>(".txt", ObjText);
        AddFunc<string, Transform>(".lua", ObjText);
        AddFunc<string, Transform>(".mp4", ObjVideo);
        AddFunc<string, Transform>(".png", ObjImage);
        AddFunc<string, Transform>(".vrm", ObjVRM);
        AddFunc<string, Transform>(".ssh", ObjSSH);
        AddFunc<string, Transform>(".obj", Obj);
    }

    /// <summary>
    /// WorldãÛä‘Ç…ObjectÇê∂ê¨Ç∑ÇÈ
    /// </summary>
    /// <param name="path"></param>
    Transform Generate(string path)
    {
        var extension = System.IO.Path.GetExtension(path);
        if (!functions.ContainsKey(extension))
        {
            return CreateObject(path, prefabUnknown).transform;
        }

        var transform = (Transform)functions[extension].DynamicInvoke(path);
        return transform;
    }

    /// <summary>
    /// ä÷êîÇìoò^Ç∑ÇÈ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="func"></param>
    void AddFunc<T1,T2>(string key, Func<T1, T2> func)
    {
        functions.Add(key, func);
    }

    /// <summary>
    /// îzíuÇ∑ÇÈèÍèäÇÃåàíË
    /// </summary>
    /// <returns></returns>
    private Vector3 GetPosition()
    {
        return db.user.position + db.user.forward;
    }

    // ------------------------------------------------
    Transform ObjText(string path)
    {
        var text = File.ReadAllText(path, System.Text.Encoding.UTF8);
        var pos = GetPosition();
        var obj = Instantiate(prefabText, pos, db.user.rotation, db.worldRoot);
        obj.SetData(path, text);
        return obj.transform;
    }

    Transform ObjVideo(string path)
    {
        var pos = GetPosition();
        var obj = Instantiate(prefabVideo, pos, db.user.rotation, db.worldRoot);
        obj.LoadVideo(path);
        return obj.transform;
    }
    
    Transform ObjImage(string path)
    {
        var data = fg.ConvertToByte(path);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(data);
        GameObject obj = CreateObject(path);

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        obj.transform.localScale *= 0.1f;

        return obj.transform;
    }
    
    Transform ObjVRM(string path)
    {
        var avatar = GM.Msg<GameObject>("VRMModelLoad", path); 
        var pos = GetPosition();
        avatar.transform.position = pos;
        avatar.transform.rotation = db.user.rotation;
        avatar.transform.SetParent(db.worldRoot);

        return avatar.transform;
    }

    Transform ObjSSH(string path)
    {        
        var data = GM.Msg<Dictionary<string, object>>("ReadYaml", path);
        
        var pos = GetPosition();
        var ssh = Instantiate(prefabSSH, pos, db.user.rotation, db.worldRoot);
        ssh.SetData(path, data);
        ssh.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        return ssh.transform;
    }
    
    Transform Obj(string path)
    {        
        var obj = GM.Msg<GameObject>("LoadObjFile", path);
        //objLoader = new ObjeLoader
        SetObjectInfo(path, obj);
        return obj.transform;
    }    

    //-----------------------------------------

    private GameObject CreateObject(string path)
    {
        GameObject obj = new GameObject();
        SetObjectInfo(path, obj);
        return obj;
    }

    private GameObject CreateObject(string path, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        SetObjectInfo(path, obj);
        return obj;
    }
    
    private void SetObjectInfo(string path, GameObject obj)
    {
        var pos = GetPosition();
        obj.transform.position = pos;
        obj.transform.rotation = db.user.rotation;
        obj.transform.SetParent(db.worldRoot);
        var objInfo = obj.AddComponent<ObjectUnknown>();
        objInfo.fileName = path;
    }

    // ------------------------------------------------    
}
