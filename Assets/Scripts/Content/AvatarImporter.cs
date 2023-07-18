using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;
using UniGLTF;
using System;
using System.IO;
using DC.Player;
using DC;

[Obsolete]
public class AvatarImporter : MonoBehaviour
{
    [SerializeField] DB_User dbUser;
    [SerializeField] Shader shader;
    [SerializeField] RuntimeAnimatorController controller;
    string avatarPath;
    
    void Start()
    {        
        GM.Add<string, bool, uint>("LoadAvatar", LoadAvatar);// idが0の場合は自身をロードする時
        avatarPath = $"{Application.dataPath}/../Avatar";
    }
    
    void LoadAvatar(string avatarId, bool isLocal, uint uid)
    {        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="mode">0:ローカル</param>
    public void GetAvatar(string avatarName, uint id, bool local)
    {
        var path = $"{avatarPath}/{avatarName}.vrm";
        if (!File.Exists(path)) return;

        var avatar = Load(path);
        var thumbnail = avatar.GetComponent<VRMMeta>().Meta.Thumbnail;
        var obj = avatar.gameObject;
        var anim = obj.GetComponent<Animator>();
        anim.runtimeAnimatorController = controller;
        //var user = dbUser.GetData(id);
        //user.thumbnail = Sprite.Create(thumbnail, new Rect(0,0,thumbnail.width,thumbnail.height), new Vector2(0.5f, 0.5f));
        //user.obj = obj;       
    }

    public RuntimeGltfInstance Load(string path, byte[] bytes = null)
    {
        GltfData data = null;
        try
        {
            if (bytes != null)
            {
                data = new GlbLowLevelParser(path, bytes).Parse();
            }
            else
            {
                data = new GlbFileParser(path).Parse();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ParseError: {path}");
            Debug.LogException(ex);
            return null;
        }

        try
        {
            using (data)
            using (var importer = new VRMImporterContext(new VRMData(data)))
            {
                var avatar = importer.Load();
                avatar.ShowMeshes();
                return avatar;
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return null;
        }
    }
}
