using MemoryPack;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCode : MonoBehaviour
{
    void Start()
    {
        var args = new Dictionary<string, object>
        {
            {"hp", 100 },
            {"name", "mao" }
        };

        var data = new P_RPC
        {
            name = "test",
            args = JsonConvert.SerializeObject(args),
        };

        var bytes = MemoryPackSerializer.Serialize(data);
        var data2 = MemoryPackSerializer.Deserialize<P_RPC>(bytes);
        Debug.Log(data2.args);
        // Debug.Log(data2.args[0]);
    }
}

[MemoryPackable]
public partial class P_RPC
{
    public string name;
    public string args;
}

