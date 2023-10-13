using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DC;
using MemoryPack;
using Unity.WebRTC;
using UnityEngine;

/// <summary>
/// TODO: 工事中
/// Peer A -> Peer C (signalingId) -> Peer C (targetId)
/// </summary>
class RTCWebRTCController : RTCControllerBase
{
    private bool isBlocking;
    // Action<string> OnConnectedPeer;

    void Start()
    {
        GM.Add<Dictionary<string, object>, string>("RPC_error", (data, sourceId) =>
        {
            GM.Msg("AddOutput", $"[Error][Receive][WebRTC]");
        });

        // GM.Add<Action<string>>("AddOnConnected", (func) => { OnConnectedPeer += func; });
        // GM.Add<string>("OnConnected", (id) => { OnConnectedPeer?.Invoke(id); });
        GM.Add<Dictionary<string, object>, string>("RPC_offer", ResponseOffer);
        GM.Add<Dictionary<string, object>, string>("RPC_answer", ResponseAnswer);
        GM.Add<Dictionary<string, object>, string>("RPC_candidateAdd", ResponseCandidate);
        GM.Add<RTCMessage, string, string>($"RECV_{nameof(MessageType.Join)}", Connect);
        GM.Add<RTCMessage, string>("WebRTCMessageHandler", OnMessageFromSignalingPeer);
    }

    /// <summary>
    /// 新規接続時にPeerに対してOfferを送信する
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="signalingId">Signalingをお願いするPeerのID</param>
    /// <param name="type"></param>
    async void Connect(RTCMessage data, string sourceId, string relayId)
    {
        var joinData = MemoryPackSerializer.Deserialize<P_Join>(data.data);

        foreach (var joinId in joinData.joinIds)
        {
            if (GM.db.rtc.peers.ContainsKey(joinId)) { continue; }
            if (joinId == relayId) continue;

            GM.Msg("AddOutput", $"[Connect] {joinId}");

            // 新規Connection作成
            AddConnection(joinId, relayId);

            // offer送信
            await UniTask.WaitWhile(() => isBlocking);
            OfferHandler(joinId, relayId);
        }

    }

    /// <summary>
    /// 中継されてきたデータを処理する
    /// </summary>
    /// <param name="data"></param>
    /// <param name="relayId"></param>
    void OnMessageFromSignalingPeer(RTCMessage data, string relayId)
    {
        var typeText = data.type.ToString(); ;
        var sourceId = data.id;
        // Debug.Log($"receive: {typeText}");
        GM.Msg($"RECV_{typeText}", data, sourceId, relayId);
    }

    void OnCandiadte(RTCIceCandidate candidate, string targetId, string signalingId)
    {
        var ice = new Ice(candidate);
        // sdp送信情報の準備
        var sendData = CreateSendData();
        sendData.Add("targetId", targetId);
        sendData.Add("candidate", ice);
        sendData.Add("type", "candidateAdd");

        // sdp送信
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
        // Remote Description 受け取る
        var remoteDesc = JsonUtility.FromJson<RTCSessionDescription>(data["sdp"].ToString());

        // Answer 送信
        var desc = await GM.db.rtc.peers[sourceId].CreateAnswer(remoteDesc);
        var sendData = CreateSendData();
        sendData.Add("sdp", desc);
        sendData.Add("targetId", sourceId);
        sendData.Add("type", "answer");

        Send(signalingId, sendData);
    }

    // -----------------------------------   


    /// <summary>
    /// Offerを受け取る
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
    /// Answerを受け取る
    /// </summary>
    /// <param name="response"></param>
    /// <param name="sourceId"></param>
    /// <param name="signalingId"></param>
    void ResponseAnswer(Dictionary<string, object> response, string sourceId)
    {
        var signalingId = response["relayId"].ToString();

        // Remote Description 受け取る
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
            // OnConnectedPeer(targetId);
        };

        connection.OnMessage += (message) =>
        {
            OnMessage(message, targetId);   // TODO: Signaling中に呼ばれないように変更すべき
        };

        connection.OnDisconnected += () =>
        {
            OnDisconnected(targetId);
        };

        GM.db.rtc.peers.Add(targetId, connection);
    }
}
