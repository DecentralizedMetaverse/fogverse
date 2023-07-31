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
    /// </summary>
    /// <param name="connectedId"></param>
    protected void OnConnected(string connectedId)
    {    
        GM.Msg("AddOutput", $"[Connected] {connectedId}");

        var sendData = CreateSendData();
        sendData.Add("type", "join");
        sendData.Add("join_id", connectedId);
        sendData.Add("target_id", "");

        foreach (var (id, peer) in GM.db.rtc.peers)
        {
            if (id == connectedId) continue;
            sendData["target_id"] = id;
            var sendDataTxt = sendData.GetString();
            GM.Msg("AddOutput", $"[Send] {sendDataTxt}");
            peer.Send(sendDataTxt);  // TODO null
        }

        GM.Msg("RequestObjectData", connectedId, "");
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

        GM.Msg("AddOutput", $"[Receive] {output}");

        // Message����
        var dataDict = output.GetDict<string, object>();
        if (dataDict.TryGetValue("type", out object value) && value.ToString() != "message")
        {
            // Log�̕\��
            var typeStr = value.ToString();
            if (typeStr != "location" && typeStr != "anim") Debug.Log(output);

            // Message�ȊO�̏���
            var targetId = dataDict["target_id"].ToString(); // TODO: KeyNotFound
            if (targetId != GM.db.rtc.id && targetId != "*")
            {
                GM.Msg("AddOutput", $"[Relay]");
                // ���p����
                Debug.Log("[Relay] ------------------");
                Send(targetId, output);
                Debug.Log("------------------");
                return;
            }

            // Message�̋�̓I�ȏ������s��
            GM.Msg("WebRTCMessageHandler", data, sourceId);
        }
    }

    protected void Send(string id, string data)
    {
        GM.Msg("AddOutput", $"[Send][WebRTC] {data}");
        GM.db.rtc.peers[id].Send(data); // TODO: Key not found
    }

    protected void OnDisconnected(string targetId)
    {
        if (!GM.db.rtc.peers.ContainsKey(targetId)) return;
        GM.Msg("AddOutput", $"[Disconnected] {targetId}");
        GM.Msg("DestroySyncObject", targetId);
        GM.db.rtc.peers.Remove(targetId);
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
            GM.db.rtc.peers[targetId].AddIceCandidate(value);
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