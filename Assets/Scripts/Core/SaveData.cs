using System.Collections.Generic;
using System.IO;
using DC;
using MemoryPack;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    string path = "";
    SaveDataContent save = new();

    void Awake()
    {
        // save.data = new();
        path = Application.persistentDataPath + "/save.dat";
        Read();
    }

    void OnDisable()
    {
        // Write();
    }

    void Start()
    {
        GM.Add("LoadSaveData", Read);
        GM.Add("Save", Write);
        GM.Add<string, object>("SetSaveData", (key, obj) =>
        {
            save.ForceAdd(key, obj);
        });
        GM.Add<string, object>("GetSaveData", (key) =>
        {
            save.TryGetValue(key, out object obj);
            return obj;
        });
    }

    public void Read()
    {
        if (!File.Exists(path))
            return;

        var txt = File.ReadAllBytes(path);
        save = MemoryPackSerializer.Deserialize<SaveDataContent>(txt);
        if (save.TryGetValue("version", out object version))
        {
            Debug.Log($"[SaveData] {version.ToString()}");
        }
    }

    public void Write()
    {
        GM.Msg("SetSaveData", "version", "0.1");

        var bin = MemoryPackSerializer.Serialize(save);
        File.WriteAllBytes(path, bin);
    }
}

[MemoryPackable(GenerateType.Collection)]
public partial class SaveDataContent : Dictionary<string, object>
{
    // public string version { get; set; }
}