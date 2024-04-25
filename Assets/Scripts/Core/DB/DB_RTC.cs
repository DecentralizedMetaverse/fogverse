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
    // ó\îıÇÃmaxPeers
    public int maxReservedPeers = 5;
    public int maxShowPeers = 5;
    public float syncIntervalTimeSecond = 0.5f;
    public float syncAnimIntervalTimeSecond = 0.5f;
    public Dictionary<string, RTCConnection> peers = new();
    //public Dictionary<string, List<RTCConnection>> groupPeers = new();
    public Dictionary<string, object> errorData = new()
    {
        { "type", "error" },
        { "reason", "" },
    };

    public Dictionary<string, List<string>> syncObjectsByID = new();
    public Dictionary<string, RTCObject> syncObjects = new();    

    public List<int> classifiedDistances = new List<int>();
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
    public Dictionary<string, GameObject> activeObjects = new();
    public void Init()
    {        
        peers.Clear();
        id = Guid.NewGuid().ToString("N");
        // GM.db.user.AddUser(id).        
    }

    public void Start()
    {
        var url = GM.Msg<object>("GetConfig", "url");
        this.url = url == null ? this.url : (string)url;
        
        var maxReservedPeers = GM.Msg<object>("GetConfig", "max-reserved-peers");
        this.maxReservedPeers = maxReservedPeers == null ? this.maxReservedPeers : int.Parse(maxReservedPeers.ToString());

        var maxShowPeers = GM.Msg<object>("GetConfig", "max-show-peers");
        this.maxShowPeers = maxShowPeers == null ? this.maxShowPeers : int.Parse(maxShowPeers.ToString());


        var distancesObj = GM.Msg<object>("GetConfig", "distances");
        if (distancesObj != null)
        {
            var distances = distancesObj.ToString().GetDict<string, float>();
            classifiedDistances.Clear();
            classifiedTimes.Clear();
            foreach (var (key, value) in distances)
            {
                var keyInt = int.Parse(key);
                classifiedDistances.Add(keyInt);
                classifiedTimes.Add(keyInt, value);
            }
        }

        // à íuèÓïÒÇÃï™óﬁÇÃèâä˙âª
        classifiedNodes.Clear();
        foreach (var key in classifiedDistances)
        {
            classifiedNodes.Add(key, new HashSet<string>());
        }

    }    

    public void End()
    {
        GM.Msg("SetConfig", "url", this.url);
        GM.Msg("SetConfig", "max-reserved-peers", this.maxReservedPeers);
        GM.Msg("SetConfig", "max-show-peers", this.maxShowPeers);
        GM.Msg("SetConfig", "distances", classifiedTimes);
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

    /// <summary>
    /// IDÇ©ÇÁRTCObjectÇéÊìæ
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public RTCObject GetSyncObjectById(string id)
    {
        syncObjectsByID.TryGetValue(id, out var ids);
        if (ids == null || ids.Count == 0)
        {
            return null;
        }
        return syncObjects[ids[0]];
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
