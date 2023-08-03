using DC;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Configuration : MonoBehaviour
{
    const string fileName = "config.json";
    string path = "";
    public Dictionary<string, object> data = new ();

    void Awake()
    {
        GM.Add<string, object>("GetConfig", (key) =>
        {
            data.TryGetValue(key, out object value);
            return value;
        });

        GM.Add<string, object>("SetConfig", (key, value) =>
        {
            data.ForceAdd(key, value);
        });

        path = $"{Application.dataPath}/../{fileName}";

        if (!File.Exists(path)) return;
        var txt = File.ReadAllText(path);
        data = txt.GetDict<string, object>();
    }

    private void OnDestroy()
    {
        var txt = data.GetString(Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(path, txt);
    }
}
