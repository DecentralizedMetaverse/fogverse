using Cysharp.Threading.Tasks;
using DC;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC;
using UnityEngine;

/// <summary>
/// TODO: �H����
/// Peer A -> Peer C (signalingId) -> Peer C (targetId)
/// </summary>
class RTCControllerWebRTC : RTCControllerBase
{
    // Dictionary<string, Action<Dictionary<string, object>, string, string>> responseFunctions = new();
    private bool isBlocking;

    void Start()
    {
        GM.Add<Dictionary<string, object>, string>("RPC_error", (data, sourceId) =>
        {
            GM.Msg("AddOutput", $"[Error][Receive][WebRTC]");
        });

        GM.Add<Dictionary<string, object>, string>("RPC_offer", ResponseOffer);
        GM.Add<Dictionary<string, object>, string>("RPC_join", Connect);
        GM.Add<Dictionary<string, object>, string>("RPC_answer", ResponseAnswer);
        GM.Add<Dictionary<string, object>, string>("RPC_candidateAdd", ResponseCandidate);
        //responseFunctions.Add("error", (_, _, _) => {  });
        //responseFunctions.Add("offer", ResponseOffer);
        //responseFunctions.Add("join", Connect);
        //responseFunctions.Add("answer", ResponseAnswer);
        //responseFunctions.Add("candidateAdd", ResponseCandidate); // TODO: ���O�̕ύX���s��        

        GM.Add<byte[], string>("WebRTCMessageHandler", OnMessageFromSignalingPeer);
    }

    /// <summary>
    /// �V�K�ڑ�����Peer�ɑ΂���Offer�𑗐M����
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="signalingId">Signaling�����肢����Peer��ID</param>
    /// <param name="type"></param>
    async void Connect(Dictionary<string, object> data, string sourceId)
    {
        var signalingId = data["relayId"].ToString();

        var joinIds = data["joinIds"].ToString().Deserialize<List<string>>();

        foreach (var joinId in joinIds)
        {
            if (GM.db.rtc.peers.ContainsKey(joinId)) { continue; }
            if (joinId == signalingId) continue;

            GM.Msg("AddOutput", $"[Connect] {joinId}");
            
            // �V�KConnection�쐬
            AddConnection(joinId, signalingId);

            // offer���M
            await UniTask.WaitWhile(() => isBlocking);
            OfferHandler(joinId, signalingId);
        }
    }

    /// <summary>
    /// ���p����Ă����f�[�^����������
    /// </summary>
    /// <param name="data"></param>
    /// <param name="relayId"></param>
    void OnMessageFromSignalingPeer(byte[] data, string relayId)
    {
        var dataMessage = MemoryPackSerializer.Deserialize<RTCMessage>(data);
        // var output = $"{System.Text.Encoding.UTF8.GetString(data)}";

        // GM.Msg("AddOutput", $"[Receive][WebRTC] {output}");

        // var dataDict = output.GetDict<string, object>();
        // if (dataDict.TryGetValue("type", out object value))
        // {
        // receive
        var typeText = dataMessage.type.ToString(); ;
        var sourceId = dataMessage.id;
        GM.Msg($"RECV_{typeText}", dataMessage, sourceId, relayId);

        //if (responseFunctions.ContainsKey(typeText))
        //{
            //responseFunctions[typeText].DynamicInvoke(dataDict, sourceId, relayId);
        //}
        //else
        //{
        //}
        // }
    }

    void OnCandiadte(RTCIceCandidate candidate, string targetId, string signalingId)
    {
        var ice = new Ice(candidate);
        // sdp���M���̏���
        var sendData = CreateSendData();
        sendData.Add("targetId", targetId);
        sendData.Add("candidate", ice);
        sendData.Add("type", "candidateAdd");

        // sdp���M
        Send(signalingId, sendData);
    }

    // -----------------------------------

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="signalingId"></param>
    protected async void OfferHandler(string targetId, string signalingId)
    {
        isBlocking = true;
        var desc = await GM.db.rtc.peers[targetId].CreateOffer();
        var sendData = CreateSendData();
        sendData.Add("type", "offer");
        sendData.Add("targetId", targetId);
        sendData.Add("sdp", desc);

        Send(signalingId, sendData); // TODO: Error
    }

    async void AnswerHandler(Dictionary<string, object> data, string sourceId, string signalingId)
    {
        // Remote Description �󂯎��
        var remoteDesc = JsonUtility.FromJson<RTCSessionDescription>(data["sdp"].ToString());

        // Answer ���M
        var desc = await GM.db.rtc.peers[sourceId].CreateAnswer(remoteDesc);
        var sendData = CreateSendData();
        sendData.Add("sdp", desc);
        sendData.Add("targetId", sourceId);
        sendData.Add("type", "answer");

        Send(signalingId, sendData);
    }

    // -----------------------------------   


    /// <summary>
    /// Offer���󂯎��
    /// </summary>
    /// <param name="response"></param>
    /// <param name="sourceId"></param>
    /// <param name="signalingId"></param>
    void ResponseOffer(Dictionary<string, object> response, string sourceId)
    {
        var relayId = response["relayId"].ToString();
        AddConnection(sourceId, relayId);

        AnswerHandler(response, sourceId, relayId);
    }

    /// <summary>
    /// Answer���󂯎��
    /// </summary>
    /// <param name="response"></param>
    /// <param name="sourceId"></param>
    /// <param name="signalingId"></param>
    void ResponseAnswer(Dictionary<string, object> response, string sourceId)
    {
        var signalingId = response["relayId"].ToString();

        // Remote Description �󂯎��
        var remoteDesc = JsonUtility.FromJson<RTCSessionDescription>(response["sdp"].ToString());
        GM.db.rtc.peers[sourceId].SetRemoteDescription(remoteDesc); // TODO: Error

        isBlocking = false;
    }

    void ResponseCandidate(Dictionary<string, object> response, string sourceId)
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
