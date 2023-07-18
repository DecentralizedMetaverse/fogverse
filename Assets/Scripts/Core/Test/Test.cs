using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var path = @"E:\Projects\research\DecentralizedMetaverse\decentralized-metaverse-unity\py-metaverse\test.ssh";
        var obj = GM.Msg<Dictionary<string, object>>("ReadYaml", path);
        var ip = obj["ip"].ToString();
        var port = int.Parse(obj["port"].ToString());
        var user = obj["user"].ToString();
        var password = obj["password"].ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
