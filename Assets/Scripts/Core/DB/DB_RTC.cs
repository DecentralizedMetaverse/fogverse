using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.WebRTC;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_RTC", menuName = "DB/DB_RTC")]
public class DB_RTC : ScriptableObject
{
    public string url = "ws://localhost:8080";
    public string id = "";
    public int maxPeers = 10;
    public float syncIntervalTimeSecond = 0.5f;
    public float syncAnimIntervalTimeSecond = 0.5f;
    public Dictionary<string, RTCConnection> peers = new();
    public Dictionary<string, List<RTCConnection>> groupPeers = new();
    public Dictionary<string, object> errorData = new()
    {
        { "type", "error" }
    };

    public Dictionary<string, List<string>> syncObjectsByID = new();
    public Dictionary<string, RTCObject> syncObjects = new();

    public int[] classifiedDistances = new int[]
    {
        3,5,7,10,20,40,
    };
    public Dictionary<int, float> classifiedTimes = new Dictionary<int, float>()
    {
        { 3, 0.1f },
        { 5, 0.25f },
        { 7, 0.5f },
        { 10, 1.0f },
        { 20, 2.0f },
        { 40, 3.0f },
    };

    // NodeÇãóó£Ç…âûÇ∂Çƒï™óﬁ
    public Dictionary<int, HashSet<string>> classifiedNodes = new();

    public void Init()
    {        
        peers.Clear();
        id = Guid.NewGuid().ToString("N");

        // à íuèÓïÒÇÃï™óﬁÇÃèâä˙âª
        classifiedNodes.Clear();
        foreach (var key in classifiedDistances)
        {
            classifiedNodes.Add(key, new HashSet<string>());
        }
    }

    public void Start()
    {
        var url = GM.Msg<object>("GetConfig", "url");
        this.url = url == null ? this.url : (string)url;
        var maxPeers = GM.Msg<object>("GetConfig", "max-peers");
        this.maxPeers = maxPeers == null ? this.maxPeers : int.Parse(maxPeers.ToString());

        GM.Msg("SetConfig", "url", this.url);
        GM.Msg("SetConfig", "max-peers", this.maxPeers);
    }

    public RTCObject selfObject
    {
        get
        {
            if (!syncObjectsByID.TryGetValue(id, out var list)) return null;
            if (list.Count == 0) return null;
            return syncObjects[list[0]];
        }
    }
}

[System.Serializable]
public class Ice
{
    public string candidate;
    public string sdpMid;
    public int sdpMLineIndex;

    public Ice(RTCIceCandidate candidate)
    {
        this.candidate = candidate.Candidate;
        sdpMid = candidate.SdpMid;
        sdpMLineIndex = (int)candidate.SdpMLineIndex;
    }

    public RTCIceCandidate Get()
    {
        var data = new RTCIceCandidate(new RTCIceCandidateInit
        {
            candidate = candidate,
            sdpMid = sdpMid,
            sdpMLineIndex = sdpMLineIndex
        });
        return data;
    }
}
