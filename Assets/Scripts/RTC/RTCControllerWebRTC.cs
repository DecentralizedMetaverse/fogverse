using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEngine;

/// <summary>
/// TODO: �H����
/// Peer A -> Peer C (signalingId) -> Peer C (targetId)
/// </summary>
class RTCControllerWebRTC : RTCControllerBase
{
    Dictionary<string, Action<Dictionary<string, object>, string, string>> responseFunctions = new();

    void Start()
    {
        responseFunctions.Add("error", (_, _, _)=> { GM.Msg("AddOutput", $"[Error][Receive][WebRTC]"); });
        responseFunctions.Add("offer", ResponseOffer);
        responseFunctions.Add("join", Connect);
        responseFunctions.Add("answer", ResponseAnswer);
        responseFunctions.Add("candidateAdd", ResponseCandidate);
        // responseFunctions.Add("location",(data, sourceId, _)=> { GM.Msg("RTCReceiveLocation", data, sourceId); });
        // responseFunctions.Add("requestObj", (data, sourceId, _)=> { GM.Msg("RTCRequstObjectInfo", data, sourceId); });
        // responseFunctions.Add("instantiate", (data, sourceId, _)=> { GM.Msg("RTCInstantiate", data, sourceId); });

        GM.Add<byte[], string>("WebRTCMessageHandler", OnMessageFromSignalingPeer);
    }    

    /// <summary>
    /// ���p����Ă����f�[�^����������
    /// </summary>
    /// <param name="data"></param>
    /// <param name="signalingId"></param>
    void OnMessageFromSignalingPeer(byte[] data, string signalingId)
    {
        var output = $"{System.Text.Encoding.UTF8.GetString(data)}";

        // GM.Msg("AddOutput", $"[Receive][WebRTC] {output}");

        var dataDict = output.GetDict<string, object>();
        if (dataDict.TryGetValue("type", out object value))
        {
            // receive
            var typeText = value.ToString();
            var sourceId = dataDict["id"].ToString();
            if (responseFunctions.ContainsKey(typeText))
            {
                responseFunctions[typeText].DynamicInvoke(dataDict, sourceId, signalingId);
            }
            else
            {

                GM.Msg($"RPC_{typeText}", dataDict, sourceId);
            }
        }
    }

    void OnCandiadte(RTCIceCandidate candidate, string targetId, string signalingId)
    {
        var ice = new Ice(candidate);
        // sdp���M���̏���
        var sendData = CreateSendData();
        sendData.Add("target_id", targetId);
        sendData.Add("candidate", ice);
        sendData.Add("type", "candidateAdd");

        // sdp���M
        Send(signalingId, sendData.GetString());
    }

    // -----------------------------------

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="signalingId"></param>
    protected async void OfferHandler(string targetId, string signalingId)
    {
        var desc = await GM.db.rtc.peers[targetId].CreateOffer();
        var sendData = CreateSendData();
        sendData.Add("type", "offer");
        sendData.Add("target_id", targetId);
        sendData.Add("sdp", desc);

        Send(signalingId, sendData.GetString()); // TODO: Error
    }

    async void AnswerHandler(Dictionary<string, object> data, string sourceId, string signalingId)
    {
        // Remote Description �󂯎��
        var remoteDesc = JsonUtility.FromJson<RTCSessionDescription>(data["sdp"].ToString());

        // Answer ���M
        var desc = await GM.db.rtc.peers[sourceId].CreateAnswer(remoteDesc);
        var sendData = CreateSendData();
        sendData.Add("sdp", desc);
        sendData.Add("target_id", sourceId);
        sendData.Add("type", "answer");

        Send(signalingId, sendData.GetString());
    }

    // -----------------------------------   
    /// <summary>
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="signalingId">Signaling�����肢����Peer��ID</param>
    /// <param name="type"></param>
    void Connect(Dictionary<string, object> data, string sourceId, string signalingId)
    {
        var joinId = data["join_id"].ToString();
        if (GM.db.rtc.peers.ContainsKey(joinId)) { return; }

        GM.Msg("AddOutput", $"[Connect] {joinId}");
        // �V�KConnection�쐬
        AddConnection(joinId, signalingId);

        // offer���M
        OfferHandler(joinId, signalingId);
    }

    /// <summary>
    /// Offer���󂯎��
    /// </summary>
    /// <param name="response"></param>
    /// <param name="sourceId"></param>
    /// <param name="signalingId"></param>
    void ResponseOffer(Dictionary<string, object> response, string sourceId, string signalingId)
    {
        AddConnection(sourceId, signalingId);

        AnswerHandler(response, sourceId, signalingId);
    }

    /// <summary>
    /// Answer���󂯎��
    /// </summary>
    /// <param name="response"></param>
    /// <param name="sourceId"></param>
    /// <param name="signalingId"></param>
    void ResponseAnswer(Dictionary<string, object> response, string sourceId, string signalingId)
    {
        // Remote Description �󂯎��
        var remoteDesc = JsonUtility.FromJson<RTCSessionDescription>(response["sdp"].ToString());
        GM.db.rtc.peers[sourceId].SetRemoteDescription(remoteDesc); // TODO: Error
    }

    void ResponseCandidate(Dictionary<string, object> response, string sourceId, string signalingId)
    {
        AddCandidate(response, sourceId);
    }

    // -----------------------------------

    private void AddConnection(string targetId, string signalingId)
    {
        if (GM.db.rtc.peers.ContainsKey(targetId)) { return; }

        var connection = new RTCConnection();
        connection.OnCandidate += (candidate) =>
        {
            OnCandiadte(candidate, targetId, signalingId);
        };
        connection.OnConnected += () =>
        {
            OnConnected(targetId);
        };

        connection.OnMessage += (message) =>
        {
            OnMessage(message, targetId);   // TODO: Signaling���ɌĂ΂�Ȃ��悤�ɕύX���ׂ�
        };

        connection.OnDisconnected += () =>
        {
            OnDisconnected(targetId);
        };

        GM.db.rtc.peers.Add(targetId, connection);
    }
}
