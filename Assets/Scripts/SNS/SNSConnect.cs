using DC;
using System.Collections.Generic;
using WebSocketSharp;
using UnityEngine;
using UnityEditor;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Text;
using System;

/// <summary>
/// Misskey Streaming APIを利用し、
/// Timelineに新しく投稿された内容を取得する
/// </summary>
public class SNSConnect : MonoBehaviour
{
    string url = "wss://misskey.io/streaming";
    WebSocket ws;
    ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
    public bool startConnect = false;

    private void Start()
    {
        GM.Add<string>("MisskeyConnect", Connect, true);
        if(startConnect) Connect(url);
    }

    void Connect(string url)
    {
        Dictionary<string, object> sendData = new()
        {
            { "type", "connect" },
            { "body", new Dictionary<string, object>
            {
                    { "channel", "globalTimeline" },
                    { "id", GUID.Generate() },
                }
            }
        };
        ws = new WebSocket(url);
        ws.OnOpen += (sender, e) =>
        {
            GM.Msg("AddOutput", $"[Misskey] Connected");
            Debug.Log("[Misskey] WebSocket Open");
        };

        ws.OnMessage += (sender, e) =>
        {
            //GM.Msg("AddOutput", $"[Misskey] {e.Data}");
            Debug.Log("[Misskey] WebSocket Message: " + e.Data);
            if (!e.IsText) { return; }
            queue.Enqueue(e.Data);
        };

        ws.OnError += (sender, e) =>
        {
            Debug.LogError($"WebSocket Error Message: {e} {e.Message}");
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket Close: " + e.Reason);
        };

        ws.Connect();
        ws.Send(sendData.GetString());
    }

    void OnDestroy()
    {
        if (ws != null) ws.Close();
    }

    void Update()
    {
        if (!queue.TryDequeue(out string message)) return;
        var data = message.GetDict<string, object>();
        var body_type = data["body"].ToString().GetDict<string, object>();
        var body = body_type["body"].ToString().GetDict<string, object>();
        var user = body["user"].ToString().GetDict<string, object>();

        var name = user["name"] != null ? user["name"].ToString() : "";
        var avatarUrl = user["avatarUrl"] != null ? user["avatarUrl"].ToString() : "";
        var text = body["text"] != null ? body["text"].ToString() : "";
        avatarUrl = RemoveQueryParam(avatarUrl, "avatar");
        var images = ExtractImageUrls(text);
        GM.Msg("AddChatView", name, avatarUrl, text, images);
    }

    public string RemoveQueryParam(string url, string paramName)
    {
        // 分割されたURLを解析
        UriBuilder uriBuilder = new UriBuilder(url);
        string query = uriBuilder.Query;

        // クエリパラメータを分割
        string[] queryParams = query.TrimStart('?').Split('&');

        // 新しいクエリパラメータを生成
        StringBuilder newQuery = new StringBuilder();
        foreach (string param in queryParams)
        {
            if (!param.StartsWith(paramName + "="))
            {
                if (newQuery.Length > 0)
                {
                    newQuery.Append('&');
                }
                newQuery.Append(param);
            }
        }

        // 新しいクエリパラメータを設定してURLを再構築
        uriBuilder.Query = newQuery.ToString();

        return uriBuilder.ToString();
    }

    public string[] ExtractImageUrls(string text)
    {
        string pattern = @"(http(s)?:\/\/.*\.(?:png|jpg|jpeg|gif|bmp))"; // 画像ファイルの拡張子に合わせて拡張子を追加
        MatchCollection matches = Regex.Matches(text, pattern);

        string[] imageUrls = new string[matches.Count];
        for (int i = 0; i < matches.Count; i++)
        {
            imageUrls[i] = matches[i].Groups[1].Value;
        }

        return imageUrls;
    }

}
