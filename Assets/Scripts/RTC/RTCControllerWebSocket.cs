using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEngine;

public class RTCControllerWebSocket : RTCControllerBase
{
    [SerializeField] SendMessageView view;
    
    protected string targetId = "";
    
    RTCConnection connection = new();
    Dictionary<string, Action<Dictionary<string, object>>> responseFunctions = new();

    void Awake()
    {
        responseFunctions.Add("offer", ResponseConnect);
        responseFunctions.Add("answer", ResponseAnswer);
        responseFunctions.Add("candidate", ResponseCandidate);

        view.address.text = GM.db.rtc.url;
        view.output.text = "";

        GM.Add("WebRTCConnectToServer", Connect);
        GM.Add<string>("WebRTCResponse", (data) =>
        {
            var receiveDataDict = data.GetDict<string, object>();
            if (receiveDataDict.TryGetValue("type", out object value))
            {
                responseFunctions[value.ToString()].DynamicInvoke(receiveDataDict);
            }
        });
    }   

    protected void Connect()
    {
        GM.Msg("AddOutput", $"[ID: {GM.db.rtc.id}]");
        // �V�KConnection�쐬
        connection = new RTCConnection();
        connection.OnCandidate += OnCandidate;

        GM.Msg("WebSocketConnect");
        var sendData = CreateSendData();
        sendData.Add("type", "connect");

        GM.Msg("WebSocketSend", sendData.GetString());
        GM.Msg("AddOutput", $"[Connect] Server");
    }
       
    void OnCandidate(RTCIceCandidate candidate)
    {
        var ice = new Ice(candidate);
        // sdp���M
        var sendData = CreateSendData();
        sendData.Add("type", "candidateAdd");
        sendData.Add("target_id", targetId);
        sendData.Add("candidate", ice);

        GM.Msg("WebSocketSend", sendData.GetString());
    }

    // -----------------------------------
    // UI�֘A
    /// <summary>
    /// �ڑ�����
    /// </summary>
    public void OnClickConnect()
    {
        GM.db.rtc.url = view.address.text;
        Connect();
    }    

    /// <summary>
    /// WebRTC�Ń��b�Z�[�W���M
    /// </summary>
    public void OnClickSend()
    {
        if (!view) return;
        var message = view.input.text;
        var sendData = CreateSendData();
        sendData.Add("message", message);

        var sendDataTxt = sendData.GetString();
        GM.Msg("AddOutput", $"[Send] {sendDataTxt}");
        foreach (var (id, peer) in GM.db.rtc.peers)
        {
            peer.Send(sendDataTxt);
        }
        // connection.Send(sendData.GetString());
    }

    // -----------------------------------
    /// <summary>
    /// TODO: �`�F�b�N
    /// </summary>
    /// <param name="response"></param>
    void ResponseConnect(Dictionary<string, object> response)
    {
        if (!response.ContainsKey("sdp"))
        {
            // offer���܂�server�ɂȂ��ꍇ
            OfferHandler();
        }
        else
        {
            // offer���󂯎��
            AnswerHandler(response);
        }
    }

    /// <summary>
    /// [Offer���M�҂̏���]
    /// </summary>
    /// <param name="response"></param>
    void ResponseAnswer(Dictionary<string, object> response)
    {
        // Remote Description �󂯎��
        var remoteDesc = JsonUtility.FromJson<RTCSessionDescription>(response["sdp"].ToString());
        targetId = response["id"].ToString();
        SetTargetId(targetId);
        connection.SetRemoteDescription(remoteDesc);
    }

    void ResponseCandidate(Dictionary<string, object> response)
    {
        AddCandidate(response, targetId);
    }

    // -----------------------------------

    async void OfferHandler()
    {
        // GM.Msg("AddOutput", "[Offer]");

        var desc = await connection.CreateOffer();
        var sendData = CreateSendData();
        sendData.Add("type", "offer");
        sendData.Add("sdp", desc);

        GM.Msg("WebSocketSend", sendData.GetString());
    }

    /// <summary>
    /// </summary>
    /// <param name="data"></param>
    async void AnswerHandler(Dictionary<string, object> data)
    {
        // GM.Msg("AddOutput", "[Answer]");
        // Remote Description �󂯎��
        var remoteDesc = JsonUtility.FromJson<RTCSessionDescription>(data["sdp"].ToString());
        targetId = data["id"].ToString();
        SetTargetId(targetId);

        // Answer ���M        
        var desc = await connection.CreateAnswer(remoteDesc);
        var sendData = CreateSendData();
        sendData.Add("type", "answer");
        sendData.Add("sdp", desc);
        sendData.Add("target_id", targetId);    // �N��answer�𑗂邩�w��
        GM.Msg("WebSocketSend", sendData.GetString());
    }
    
    // -----------------------------------
    private void SetTargetId(string targetId)
    {
        this.targetId = targetId;
        connection.OnConnected += () =>
        {
            OnConnected(targetId);
        };
        connection.OnMessage += (message) =>
        {
            OnMessage(message, targetId);
        };
        connection.OnDisconnected += () =>
        {
            OnDisconnected(targetId);
        };

        GM.db.rtc.peers.Add(targetId, connection);  // TODO: targetId is ""
    }
}
