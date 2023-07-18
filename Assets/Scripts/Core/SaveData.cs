using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
using System.Linq;
using DC;

public class SaveData : MonoBehaviour
{
    string path = "";
    Dictionary<string, object> data = new();

    void Awake()
    {
        path = Application.persistentDataPath + "/save.dat";
        Read();
    }

    void OnDisable()
    {
        Write();
    }

    void Start()
    {
        GM.Add("LoadSaveData", Read);
        GM.Add("Save", Write);
        GM.Add<string, object>("SetSaveData", (key, obj) =>
        {
            data.ForceAdd(key, obj);
        });
        GM.Add<string, object>("GetSaveData", (key) =>
        {
            data.TryGetValue(key, out object obj);
            return obj;
        });
    }

    public void Read()
    {
        if (!File.Exists(path))
            return;

        data = File.ReadAllText(path).GetDict<string, object>();

    }

    public void Write()
    {
        File.WriteAllText(path, data.GetString());
    }
}