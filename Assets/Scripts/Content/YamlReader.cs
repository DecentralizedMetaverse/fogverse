using System.Collections.Generic;
using System.IO;
using DC;
using UnityEngine;
using YamlDotNet.Serialization;

/// <summary>
/// YamlFile“Ç‚İ‘‚«
/// </summary>
public class YamlReader : MonoBehaviour
{
    void Start()
    {
        GM.Add<string, Dictionary<string, object>>("ReadYaml", Read);
        GM.Add<string, Dictionary<string, object>>("ReadYamlText", ReadText);
        GM.Add<string, Dictionary<string, object>>("WriteYaml", Write);
    }

    Dictionary<string, object> Read(string path)
    {
        if(!File.Exists(path)) return null;

        var input = File.ReadAllText(path);
        var result = ReadText(input);
        
        return result;
    }

    Dictionary<string, object> ReadText(string input)
    {
        var deserializer = new DeserializerBuilder().Build();
        var result = deserializer.Deserialize<Dictionary<string, object>>(input);

        return result;
    }

    /// <summary>
    /// Yaml‘‚«‚İ
    /// </summary>
    /// <param name="data"></param>
    /// <returns>•Û‘¶‚µ‚½FileƒpƒX</returns>
    void Write(string path, Dictionary<string, object> data)
    {
        var serializer = new SerializerBuilder().Build();
        File.WriteAllText(path, serializer.Serialize(data));                
    }
}
