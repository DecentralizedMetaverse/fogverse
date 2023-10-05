using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DC;
using MemoryPack;
using UnityEngine;

public class RTCControllerBase : MonoBehaviour
{
    protected HashSet<string> candidateData = new();
    //HashSet<MessageType> logTargetExcludeTypes = new HashSet<MessageType>()
    //{ 
    //    MessageType.Location
    //};

    /// <summary>
    /// 接続完了時
    /// </summary>
    /// <param name="connectedId"></param>
    protected async void OnConnected(string connectedId)
    {
        GM.Msg("AddOutput", $"[Connected] {connectedId}");

        await UniTask.Delay(1000); // TODO: 暫定的な措置

        // 接続先の情報を全体に送信        
        GM.Msg("RTCSendDirect", MessageType.GetUserList, connectedId, null);

        await UniTask.Yield();

        // UserObjectの情報を取得する
        GM.Msg("UserObjectInfoRequest", connectedId);
    }

    /// <summary>
    /// Message受信時の処理
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    protected void OnMessage(byte[] data, string sourceId)
    {
        // Message処理
        var dataMessage = MemoryPackSerializer.Deserialize<RTCMessage>(data);
        if (dataMessage.id == null)
        {
            Debug.LogError(System.Text.Encoding.UTF8.GetString(data));
            return;
        }
        // Message以外の処理
        var targetId = dataMessage.targetId;
        if (IsRelayData(targetId))
        {
            // 中継する                
            GM.Msg("AddOutput", $"[Relay]");
            Send(targetId, data);
        }
        else
        {
            // Messageの具体的な処理を行う
            GM.Msg("WebRTCMessageHandler", dataMessage, sourceId);
        }
    }

    private static bool IsRelayData(string targetId)
    {
        return targetId != GM.db.rtc.id && targetId != "*";
    }

    protected void Send(string id, string data)
    {
        GM.Msg("AddOutput", $"[Send][WebRTC] {data}");
        GM.db.rtc.peers[id].Send(data);
    }

    protected void Send(string id, Dictionary<string, object> data)
    {
        GM.Msg("AddOutput", $"[Send][WebRTC] {data}");
        // AddOutputLog(data, "Send");
        GM.db.rtc.peers[id].Send(data.GetString());
    }

    protected void Send(string id, byte[] data)
    {
        GM.Msg("AddOutput", $"[Send][WebRTC] {data}");
        GM.db.rtc.peers[id].Send(data);
    }

    protected void OnDisconnected(string targetId)
    {
        if (!GM.db.rtc.peers.ContainsKey(targetId)) return;
        GM.Msg("AddOutput", $"[Disconnected] {targetId}");
        GM.Msg("DestroySyncObject", targetId);
        GM.db.rtc.peers.Remove(targetId);
    }

    //void AddOutputLog(Dictionary<string, object> data, string header = "Send")
    //{
    //    var typeStr = data["type"].ToString();
    //    if (typeStr != "location" && typeStr != "anim" && typeStr != "ping" && typeStr != "pong")
    //    {
    //        var dataStr = data.GetString();
    //        Debug.Log(dataStr);
    //        GM.Msg("AddOutput", $"[{header}] {dataStr}");
    //    }
    //}

    // -----------------------------------

    /// <summary>
    /// 経路候補を登録する
    /// </summary>
    /// <param name="data"></param>
    protected void AddCandidate(Dictionary<string, object> data, string targetId)
    {
        var dataStr = data["candidate"].ToString();

        var candidates = dataStr.Split("|");

        foreach (var candidate in candidates)
        {
            if (candidateData.Contains(candidate)) continue;

            var value = JsonUtility.FromJson<Ice>(candidate);
            candidateData.Add(candidate);
            GM.db.rtc.peers[targetId].AddIceCandidate(value); // TODO: null
        }
    }

    // -----------------------------------

    /// <summary>
    /// 送信用データを作成する
    /// </summary>
    /// <returns></returns>
    protected Dictionary<string, object> CreateSendData()
    {
        var sendData = new Dictionary<string, object>
        {
            { "id", GM.db.rtc.id },
        };

        return sendData;
    }
}