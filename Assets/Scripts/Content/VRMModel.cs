using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRM;

public class VRMModel : MonoBehaviour
{
    void Start()
    {
        GM.Add<string, GameObject>("VRMModelLoad", (path) => { return Load(path, null); });
    }

    public GameObject Load(string path, byte[] bytes = null)
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
                return avatar.gameObject;
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return null;
        }
    }
}
