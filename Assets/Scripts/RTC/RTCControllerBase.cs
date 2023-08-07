using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEngine;

public abstract class RTCControllerBase : MonoBehaviour
{
    protected HashSet<string> candidateData = new();

    /// <summary>
    /// �ڑ�������
    /// TODO: �SClient�𑗐M����悤�ɕύX����
    /// </summary>
    /// <param name="connectedId"></param>
    protected async void OnConnected(string connectedId)
    {
        GM.Msg("AddOutput", $"[Connected] {connectedId}");

        await UniTask.Delay(1000); // TODO: �b��I�ȑ[�u

        // �ڑ���̏���S�̂ɑ��M        
        Dictionary<string, object> sendData = new()
        {
            {"type", "get_user_list"},
        };        
        GM.Msg("RTCSendDirect", connectedId, sendData);
    }

    /// <summary>
    /// Message��M���̏���
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    protected void OnMessage(byte[] data, string sourceId)
    {
        var output = $"{System.Text.Encoding.UTF8.GetString(data)}";
        // Debug.Log(output);


        // Message����
        var dataDict = output.GetDict<string, object>();
        if (dataDict.TryGetValue("type", out object value))
        {
            // Log�̕\��
            AddOutputLog(dataDict, "Receive");

            // Message�ȊO�̏���
            var targetId = dataDict["target_id"].ToString(); // TODO: KeyNotFound
            if (targetId != GM.db.rtc.id && targetId != "*")
            {
                GM.Msg("AddOutput", $"[Relay]");
                // ���p����                
                Send(targetId, data);
                return;
            }

            // Message�̋�̓I�ȏ������s��
            GM.Msg("WebRTCMessageHandler", data, sourceId);
        }
    }

    protected void Send(string id, string data)
    {
        GM.Msg("AddOutput", $"[Send][WebRTC] {data}");
        GM.db.rtc.peers[id].Send(data);
    }

    protected void Send(string id, Dictionary<string, object> data)
    {
        GM.Msg("AddOutput", $"[Send][WebRTC] {data}");
        AddOutputLog(data, "Send");
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

    void AddOutputLog(Dictionary<string, object> data, string header = "Send")
    {
        var typeStr = data["type"].ToString();
        if (typeStr != "location" && typeStr != "anim" && typeStr != "ping" && typeStr != "pong")
        {
            var dataStr = data.GetString();
            Debug.Log(dataStr);
            GM.Msg("AddOutput", $"[{header}] {dataStr}");
        }
    }

    // -----------------------------------

    /// <summary>
    /// �o�H����o�^����
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
    /// ���M�p�f�[�^���쐬����
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