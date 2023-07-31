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

    public void Init()
    {
        peers.Clear();
        id = Guid.NewGuid().ToString("N");
    }

    public RTCObject mineObject
    {
        get
        {
            if(!syncObjectsByID.TryGetValue(id, out var list)) return null;
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
