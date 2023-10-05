using Cysharp.Threading.Tasks;
using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.UI;

public class WebRequest : MonoBehaviour
{
    string result = "";
    void Start()
    {
        GM.Add<string, Dictionary<string, object>>("ReadJsonText", StringToDict, true);
        GM.Add<string, UniTask<string>>("WebRequestGet", Get, true);
        GM.Add<string>("WebRequestGetResult", () => { return result; }, true);
        GM.Add<string, Dictionary<string, object>, UniTask<string>>("WebRequestPut", Put, true);
        GM.Add<string, Dictionary<string, object>, UniTask<string>>("WebRequestPost", Post, true);
    }

    async UniTask<string> Get(string url)
    {
        var token = this.GetCancellationTokenOnDestroy();
        using (var webRequest = UnityWebRequest.Get(url))
        {
            try
            {
                await webRequest.SendWebRequest().WithCancellation(token);
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                }

                var ret = webRequest.downloadHandler.text;
                // Success
                result = ret;
                Debug.Log(ret);
                return ret;
            }
            catch (OperationCanceledException ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        return "";
    }

    async UniTask<string> Post(string url, Dictionary<string, object> data)
    {
        // キャンセルトークンの取得
        var token = this.GetCancellationTokenOnDestroy();

        var json = JsonConvert.SerializeObject(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // UnityWebRequest.Postでリクエストを作成
        using (var webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            try
            {
                // リクエストを送信し、キャンセル可能にする
                await webRequest.SendWebRequest().WithCancellation(token);
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error While Sending: " + webRequest.error);
                }

                var ret = webRequest.downloadHandler.text;
                // Success
                result = ret;
                Debug.Log(ret);
                return ret;
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合の処理
                Debug.Log("Canceled");
            }

            return "";
        }
    }

    async UniTask<string> Put(string url, Dictionary<string, object> data)
    {
        // キャンセルトークンの取得
        var token = this.GetCancellationTokenOnDestroy();

        var json = JsonConvert.SerializeObject(data);
        // 文字列をbyte配列に変換する
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // UnityWebRequest.Postでリクエストを作成
        using (var webRequest = UnityWebRequest.Put(url, bodyRaw))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");

            try
            {
                // リクエストを送信し、キャンセル可能にする
                await webRequest.SendWebRequest().WithCancellation(token);
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error While Sending: " + webRequest.error);
                }

                var ret = webRequest.downloadHandler.text;
                // Success
                result = ret;
                Debug.Log(ret);
                return ret;
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合の処理
                Debug.Log("Canceled");
            }

            return "";
        }
    }

    Dictionary<string, object> StringToDict(string json)
    {
        // JsonからDictionaryに変換
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        return dictionary;
    }
}
