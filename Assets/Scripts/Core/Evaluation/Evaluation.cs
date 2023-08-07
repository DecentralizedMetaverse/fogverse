using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using DC;
using System;

public class Evaluation : MonoBehaviour
{
    public float intervalTimeSec = 1f;
    [SerializeField] TMP_Text textId;
    [SerializeField] TMP_Text textFps;
    [SerializeField] TMP_Text textStaticObject;
    [SerializeField] TMP_Text textDynamicObject;
    [SerializeField] TMP_Text textPing;
    List<EvaluationData> data = new List<EvaluationData>();
    private float time;
    string fileName = "evaluation.csv";
    string path = "";
    EvaluationData currentData = new EvaluationData();

    Dictionary<string, object> sendPingData = new();
    DateTime sendPingTime;

    private void Start()
    {
        GM.Add<string>("GetEvaluationData", () =>
        {
            return data.GetString();
        });

        GM.Add<Dictionary<string, object>, string>("RPC_ping", RPCPing);
        GM.Add<Dictionary<string, object>, string>("RPC_pong", RPCPong);
        GM.Add<float>("SetFPS", (fps) => currentData.fps = fps);
        sendPingData.Add("type", "ping");
    }

    private void OnDestroy()
    {
        path = $"{Application.dataPath}/{GM.mng.outputPath}/{DateTime.Now.ToString("yyyy-MM-dd-HH-mm")}-{GM.db.rtc.id}-{fileName}";
        System.IO.File.WriteAllText(path, data.GetString());
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time < intervalTimeSec) return;
        time = 0;

        currentData.staticObject = GM.db.player.worldRoot != null ? GM.db.player.worldRoot.childCount : 0;
        currentData.dynamicObject = GM.db.rtc.peers.Count;

        data.Add(currentData);
        // currentData = new EvaluationData();

        // pingの送信
        if (GM.db.rtc.peers.Count == 0) return;
        sendPingTime = DateTime.Now;
        GM.Msg("RTCSendAll", sendPingData);
        UpdateText();
    }

    /// <summary>
    /// TODO: 確認が必要
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    private void RPCPing(Dictionary<string, object> data, string sourceId)
    {
        data["type"] = "pong";
        GM.Msg("RTCSendDirect", sourceId, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sourceId"></param>
    private void RPCPong(Dictionary<string, object> data, string sourceId)
    {
        var ping = DateTime.Now - sendPingTime;
        if (currentData.ping == null) currentData.ping = new Dictionary<string, object>();
        currentData.ping.ForceAdd(sourceId, ping);
    }

    void UpdateText()
    {
        textId.text = GM.db.rtc.id;
        textFps.text = currentData.fps.ToString();
        textStaticObject.text = currentData.staticObject.ToString();
        textDynamicObject.text = currentData.dynamicObject.ToString();

        var pingTxt = "";

        //foreach (var (key, peer) in GM.db.rtc.peers)
        //{
        //    if (peer.pc == null) continue;
        //    var stats = peer.pc.GetStats();
        //    pingTxt += $"{key}: {stats.}\n";
        //}

        if (currentData.ping == null) return;
        foreach (var item in currentData.ping)
        {
            pingTxt += $"[{item.Key}] {item.Value} ";
        }
        textPing.text = pingTxt;
        currentData.pingTxt = pingTxt;
    }
}

public struct EvaluationData
{
    public float fps { get; set; }
    public int staticObject { get; set; }
    public int dynamicObject { get; set; }
    public string pingTxt { get; set; }
    public Dictionary<string, object> ping { get; set; }
}
