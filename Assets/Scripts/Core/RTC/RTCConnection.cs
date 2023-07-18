using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEngine;

public class RTCConnection
{
    public string id;
    string[] stunUrls = new string[] { "stun:stun.l.google.com:19302" };
    private RTCPeerConnection pc;   // TODO: Peerごとに必要
    private RTCDataChannel dataChannel;
    public Action<byte[]> OnMessage;
    public Action<RTCIceCandidate> OnCandidate;
    public Action OnConnected, OnDisconnected;

    public async UniTask<RTCSessionDescription> CreateOffer()
    {
        Debug.Log($"[{id}][CreateOffer]");

        // ----------------------------
        // Configuration
        var configuration = default(RTCConfiguration);
        configuration.iceServers = new RTCIceServer[]
        {
            new RTCIceServer{urls = stunUrls}
        };


        pc = new RTCPeerConnection(ref configuration);

        // ----------------------------
        // Candidate
        pc.OnIceCandidate = OnIceCandidate;
        pc.OnIceConnectionChange = OnIceConnectionChange;

        SetDataChannel();

        // ----------------------------
        // CreateOffer
        var offerOperation = pc.CreateOffer();
        await offerOperation;
        if (offerOperation.IsError)
        {
            Debug.LogError($"[{id}][Error][OfferOperation]");
        }

        // ----------------------------
        // LocalDescription
        var desc = offerOperation.Desc;
        var localDescriptionOperation = pc.SetLocalDescription(ref desc);
        if (localDescriptionOperation.IsError)
        {
            Debug.LogError($"[{id}][Error][SetLocalDescription]");
        }

        return desc;
    }

    public async UniTask<RTCSessionDescription> CreateAnswer(RTCSessionDescription remoteDescription)
    {
        Debug.Log($"[{id}][CreateAnswer]");

        // ----------------------------
        // Configuration
        var configuration = default(RTCConfiguration);
        configuration.iceServers = new RTCIceServer[]
        {
            new RTCIceServer{urls = stunUrls}
        };
        pc = new RTCPeerConnection(ref configuration);

        // ----------------------------
        // Candiate        
        pc.OnIceCandidate = OnIceCandidate;
        pc.OnIceConnectionChange = OnIceConnectionChange;


        // ----------------------------
        // DataChannels
        pc.OnDataChannel = channel =>
        {
            dataChannel = channel;
            dataChannel.OnMessage = OnMessageDataChannel;
        };

        // ----------------------------
        // RemoteDescription
        var remoteDescriptionOperation = pc.SetRemoteDescription(ref remoteDescription);
        await remoteDescriptionOperation;
        if (remoteDescriptionOperation.IsError)
        {
            Debug.LogError($"[{id}][Error][SetRemoteDescription] {remoteDescriptionOperation.Error.message}");
        }

        // ----------------------------
        // CreateAnswer
        var answerOperation = pc.CreateAnswer();
        await answerOperation;
        if (answerOperation.IsError)
        {
            Debug.LogError($"[{id}][Error][CreateAnswer]");
        }

        // ----------------------------
        // LocalDescription
        var desc = answerOperation.Desc;
        var localDescriptionOperation = pc.SetLocalDescription(ref desc);
        if (localDescriptionOperation.IsError)
        {
            Debug.LogError($"[{id}][Error][SetLocalDescription]");
        }

        return desc;
    }

    public async void SetRemoteDescription(RTCSessionDescription remoteDescription)
    {
        Debug.Log($"[{id}][SetRemoteDescription]");

        var remoteDescriptionOperation = pc.SetRemoteDescription(ref remoteDescription);
        await remoteDescriptionOperation;
        if (remoteDescriptionOperation.IsError)
        {
            Debug.LogError($"[{id}][Error][SetRemoteDescription] {remoteDescriptionOperation.Error.message}");
        }
    }

    public void AddIceCandidate(Ice candidate)
    {
        pc.AddIceCandidate(candidate.Get());
    }

    public void Send(string data)
    {
        if (dataChannel == null)
        {
            // WebRTC交換時において、まだ接続が確立していないときがある
            Debug.LogWarning("dataChannel is null");
            return;
        }
        else if (dataChannel.ReadyState != RTCDataChannelState.Open)
        {
            Debug.LogWarning("dataChannel is not open");
            return;
        }

        dataChannel.Send(data);
    }

    private void SetDataChannel()
    {
        var config = new RTCDataChannelInit();
        dataChannel = pc.CreateDataChannel("data", config); // Create Data Channel
        dataChannel.OnMessage = OnMessageDataChannel;
        dataChannel.OnOpen = OnOpenDataChannel;
        dataChannel.OnClose = OnCloseDataChannel;
    }

    void OnIceConnectionChange(RTCIceConnectionState state)
    {
        Debug.Log($"[{id}][ConnectionChange] {state.ToString()}");

        if (state == RTCIceConnectionState.Connected)
        {
            OnConnected?.Invoke();
        }
        else if (state == RTCIceConnectionState.Disconnected)
        {
            OnDisconnected?.Invoke();
        }
    }

    void OnOpenDataChannel()
    {
        Debug.Log($"[{id}][DataChannel] Open");
    }

    void OnCloseDataChannel()
    {
        Debug.Log($"[{id}][DataChannel] Close");
    }

    void OnMessageDataChannel(byte[] data)
    {
        OnMessage?.Invoke(data);
    }

    private void OnIceCandidate(RTCIceCandidate candidate)
    {
        Debug.Log($"[{id}][OnIceCandidate] {candidate.Candidate}");
        OnCandidate?.Invoke(candidate);
    }
}
