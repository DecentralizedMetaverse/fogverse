using System.Collections.Generic;
using System.IO;
using DC;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    string path = "";
    Dictionary<string, object> data = new();

    void Awake()
    {
        // save.data = new();
        path = Application.persistentDataPath + "/save.dat";
        Read();
    }

    void OnDestroy()
    {
        Write();
    }

    void Start()
    {
        GM.Add("LoadSaveData", Read);
        GM.Add("Save", Write);
        GM.Add<string, object>("SetSaveData", (key, obj) =>
        {
            if (data == null) data = new();
            data.ForceAdd(key, obj);
        });
        GM.Add<string, object>("GetSaveData", (key) =>
        {
            if (data == null)
            {
                Debug.LogWarning($"{key} not found in SaveData");
                return null;
            }
            data.TryGetValue(key, out object obj);
            return obj;
        });
    }

    public void Read()
    {
        if (!File.Exists(path))
            return;

        var txt = File.ReadAllText(path);
        data = txt.GetDict<string, object>();
        if (data == null) return;

        if (data.TryGetValue("version", out object version))
        {
            Debug.Log($"[SaveData] {version.ToString()}");
        }
    }

    public void Write()
    {
        GM.Msg("SetSaveData", "version", "0.1");

        var txt = data.GetString();
        File.WriteAllText(path, txt);
    }
}