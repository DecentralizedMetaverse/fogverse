using Cysharp.Threading.Tasks;
using DC;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WebSocketSharp;

public class WebSocketManager : MonoBehaviour
{
    private WebSocket ws;

    void Start()
    {
        GM.Add<string>("WebSocketSetAddress", (address) => { GM.db.rtc.id = address; }, true);
        GM.Add("WebSocketConnect", Connect, true);
        GM.Add("WebSocketClose", Close, true);
        GM.Add<string>("WebSocketSend", Send, true);
    }

    private void OnDestroy()
    {
        Close();
    }

    void Close()
    {
        if (ws != null) ws.Close();
    }

    void Connect()
    {
        ws = new WebSocket(GM.db.rtc.url);
        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open");
            // ws.Send(Encoding.UTF8.GetBytes("hello"));
        };

        ws.OnMessage += (sender, e) =>
        {
            GM.Msg("WebRTCResponse", e.Data);
            GM.Msg("AddOutput", $"[Receive] {e.Data}");
        };

        ws.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket Error Message: " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket Close: " + e.Reason);
        };

        ws.Connect();
    }

    void Send(string message)
    {
        GM.Msg("AddOutput", $"[Send] {message}");
        ws.Send(Encoding.UTF8.GetBytes(message));
    }
}
